using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FileManager.NamingTemplate;

public class NamingTemplate
{
	public string TemplateText { get; private set; }
	public IEnumerable<ITemplateTag> TagsInUse => _tagsInUse;
	public IEnumerable<ITemplateTag> TagsRegistered => Classes.SelectMany(p => p.TemplateTags).DistinctBy(f => f.TagName);
	public IEnumerable<string> Warnings => errors.Concat(warnings);
	public IEnumerable<string> Errors => errors;

	private Delegate templateToString;
	private readonly List<string> warnings = new();
	private readonly List<string> errors = new();
	private readonly IEnumerable<TagClass> Classes;
	private readonly List<ITemplateTag> _tagsInUse = new();

	public const string ERROR_NULL_IS_INVALID = "Null template is invalid.";
	public const string WARNING_EMPTY = "Template is empty.";
	public const string WARNING_WHITE_SPACE = "Template is white space.";
	public const string WARNING_NO_TAGS = "Should use tags. Eg: <title>";

	/// <summary>
	/// Invoke the <see cref="NamingTemplate"/> to  
	/// </summary>
	/// <param name="propertyClasses">Instances of the TClass used in <see cref="PropertyTagClass{TClass}"/> and <see cref="ConditionalTagClass{TClass}"/></param>
	/// <returns></returns>
	public TemplatePart Evaluate(params object[] propertyClasses)
	{
		//Match propertyClasses to the arguments required by templateToString.DynamicInvoke()
		var delegateArgTypes = templateToString.GetType().GenericTypeArguments[..^1];

		object[] args = new object[delegateArgTypes.Length];

		for (int i = 0; i < delegateArgTypes.Length; i++)
			args[i] = propertyClasses.First(o => o.GetType() == delegateArgTypes[i]);

		if (args.Any(a => a is null))
			throw new ArgumentException($"This instance of {nameof(NamingTemplate)} requires the following arguments: {string.Join(", ", delegateArgTypes.Select(t => t.Name).Distinct())}");

		return ((TemplatePart)templateToString.DynamicInvoke(args)).FirstPart;
	}

	/// <summary>Parse a template string to a <see cref="NamingTemplate"/></summary>
	/// <param name="template">The template string to parse</param>
	/// <param name="tagClasses">A collection of <see cref="ITagClass"/> with
	/// properties registered to match to the <paramref name="template"/></param>
	public static NamingTemplate Parse(string template, IEnumerable<TagClass> tagClasses)
	{
		var namingTemplate = new NamingTemplate(tagClasses);
		try
		{
			BinaryNode intermediate = namingTemplate.IntermediateParse(template);
			Expression evalTree = GetExpressionTree(intermediate);

			List<ParameterExpression> parameters = new();

			foreach (var tagclass in tagClasses)
					parameters.Add(tagclass.Parameter);

			namingTemplate.templateToString = Expression.Lambda(evalTree, parameters).Compile();
		}
		catch(Exception ex)
		{
			namingTemplate.errors.Add(ex.Message);
		}
		return namingTemplate;
	}

	private NamingTemplate(IEnumerable<TagClass> properties)
	{
		Classes = properties;
	}

	/// <summary>Builds an <see cref="Expression"/> tree that will evaluate to a <see cref="TemplatePart"/></summary>
	private static Expression GetExpressionTree(BinaryNode node)
	{
		if (node is null) return TemplatePart.Blank;
		else if (node.IsValue) return node.Expression;
		else if (node.IsConditional) return Expression.Condition(node.Expression, concatExpression(node), TemplatePart.Blank);
		else return concatExpression(node);

		Expression concatExpression(BinaryNode node)
			=> TemplatePart.CreateConcatenation(GetExpressionTree(node.LeftChild), GetExpressionTree(node.RightChild));
	}

	/// <summary>Parse a template string into a <see cref="BinaryNode"/> tree</summary>
	private BinaryNode IntermediateParse(string templateString)
	{
		if (templateString is null)
			throw new NullReferenceException(ERROR_NULL_IS_INVALID);
		else if (string.IsNullOrEmpty(templateString))
			warnings.Add(WARNING_EMPTY);
		else if (string.IsNullOrWhiteSpace(templateString))
			warnings.Add(WARNING_WHITE_SPACE);

		TemplateText = templateString;

		BinaryNode currentNode = BinaryNode.CreateRoot();
		BinaryNode topNode = currentNode;
		List<char> literalChars = new();

		while (templateString.Length > 0)
		{
			if (StartsWith(templateString, out string exactPropertyName, out var propertyTag, out var valueExpression))
			{
				checkAndAddLiterals();

				if (propertyTag is IClosingPropertyTag)
					currentNode = AddNewNode(currentNode, BinaryNode.CreateConditional(propertyTag.TemplateTag, valueExpression));
				else
				{
					currentNode = AddNewNode(currentNode, BinaryNode.CreateValue(propertyTag.TemplateTag, valueExpression));
					_tagsInUse.Add(propertyTag.TemplateTag);
				}

				templateString = templateString[exactPropertyName.Length..];
			}
			else if (StartsWithClosing(templateString, out exactPropertyName, out var closingPropertyTag))
			{
				checkAndAddLiterals();

				BinaryNode lastParenth = currentNode;

				while (lastParenth?.IsConditional is false)
					lastParenth = lastParenth.Parent;

				if (lastParenth?.Parent is null)
				{
					warnings.Add($"Missing <{closingPropertyTag.TemplateTag.TagName}-> open conditional.");
					break;
				}
				else if (lastParenth.Name != closingPropertyTag.TemplateTag.TagName)
				{
					warnings.Add($"Missing <-{lastParenth.Name}> closing conditional.");
					break;
				}

				currentNode = lastParenth.Parent;
				templateString = templateString[exactPropertyName.Length..];
			}
			else
			{
				//templateString does not start with a tag, so the first
				//character is a literal and not part of a tag expression.
				literalChars.Add(templateString[0]);
				templateString = templateString[1..];
			}
		}
		checkAndAddLiterals();

		//Check for any conditionals that haven't been closed
		while (currentNode is not null)
		{
			if (currentNode.IsConditional)
				warnings.Add($"Missing <-{currentNode.Name}> closing conditional.");
			currentNode = currentNode.Parent;
		}

		if (!_tagsInUse.Any())
			warnings.Add(WARNING_NO_TAGS);

		return topNode;

		void checkAndAddLiterals()
		{
			if (literalChars.Count != 0)
			{
				currentNode = AddNewNode(currentNode, BinaryNode.CreateValue(new string(literalChars.ToArray())));
				literalChars.Clear();
			}
		}
	}

	private bool StartsWith(string template, out string exactName, out IPropertyTag propertyTag, out Expression valueExpression)
	{
		foreach (var pc in Classes)
		{
			if (pc.StartsWith(template, out exactName, out propertyTag, out valueExpression))
				return true;
		}
		exactName = null;
		valueExpression = null;
		propertyTag = null;
		return false;
	}

	private bool StartsWithClosing(string template, out string exactName, out IClosingPropertyTag closingPropertyTag)
	{
		foreach (var pc in Classes)
		{
			if (pc.StartsWithClosing(template, out exactName, out closingPropertyTag))
				return true;
		}
		exactName = null;
		closingPropertyTag = null;
		return false;
	}

	private static BinaryNode AddNewNode(BinaryNode currentNode, BinaryNode newNode)
	{
		if (currentNode.LeftChild is null)
		{
			newNode.Parent = currentNode;
			currentNode.LeftChild = newNode;
		}
		else if (currentNode.RightChild is null)
		{
			newNode.Parent = currentNode;
			currentNode.RightChild = newNode;
		}
		else
		{
			currentNode.RightChild = BinaryNode.CreateConcatenation(currentNode.RightChild, newNode);
			currentNode.RightChild.Parent = currentNode;
			currentNode = currentNode.RightChild;
		}

		return newNode.IsConditional ? newNode : currentNode;
	}

	private class BinaryNode
	{
		public string Name { get; }
		public BinaryNode Parent { get; set; }
		public BinaryNode RightChild { get; set; }
		public BinaryNode LeftChild { get; set; }
		public Expression Expression { get; private init; }
		public bool IsConditional { get; private init; } = false;
		public bool IsValue { get; private init; } = false;

		public static BinaryNode CreateRoot() => new("Root");

		public static BinaryNode CreateValue(string literal) => new("Literal")
		{
			IsValue = true,
			Expression = TemplatePart.CreateLiteral(literal)
		};

		public static BinaryNode CreateValue(ITemplateTag templateTag, Expression property) => new(templateTag.TagName)
		{
			IsValue = true,
			Expression = TemplatePart.CreateProperty(templateTag, property)
		};

		public static BinaryNode CreateConditional(ITemplateTag templateTag, Expression property) => new(templateTag.TagName)
		{
			IsConditional = true,
			Expression = property
		};

		public static BinaryNode CreateConcatenation(BinaryNode left, BinaryNode right)
		{
			var newNode = new BinaryNode("Concatenation")
			{
				LeftChild = left,
				RightChild = right
			};
			newNode.LeftChild.Parent = newNode;
			newNode.RightChild.Parent = newNode;
			return newNode;
		}

		private BinaryNode(string name) => Name = name;
		public override string ToString() => Name;
	}
}
