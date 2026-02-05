using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace FileManager.NamingTemplate;

public class NamingTemplate
{
	public string TemplateText { get; private set; } = string.Empty;
	public IEnumerable<ITemplateTag> TagsInUse => _tagsInUse;
	public IEnumerable<ITemplateTag> TagsRegistered => TagCollections.SelectMany(t => t).DistinctBy(t => t.TagName);
	public IEnumerable<string> Warnings => errors.Concat(warnings);
	public IEnumerable<string> Errors => errors;

	private Delegate? templateToString;
	private readonly List<string> warnings = new();
	private readonly List<string> errors = new();
	private readonly IEnumerable<TagCollection> TagCollections;
	private readonly List<ITemplateTag> _tagsInUse = new();

	public const string ERROR_NULL_IS_INVALID = "Null template is invalid.";
	public const string WARNING_EMPTY = "Template is empty.";
	public const string WARNING_WHITE_SPACE = "Template is white space.";
	public const string WARNING_NO_TAGS = "Should use tags. Eg: <title>";

	/// <summary>
	/// Invoke the <see cref="NamingTemplate"/>
	/// </summary>
	/// <param name="propertyClasses">Instances of the TClass used in <see cref="PropertyTagCollection{TClass}"/> and <see cref="ConditionalTagCollection{TClass}"/></param>
	public TemplatePart Evaluate(params object?[] propertyClasses)
	{
		if (templateToString is null)
			throw new InvalidOperationException();

		// Match propertyClasses to the arguments required by templateToString.DynamicInvoke(). 
		// First parameter is "this", so ignore it.
		var delegateArgTypes = templateToString.Method.GetParameters().Skip(1);
		
		object?[] args = delegateArgTypes.Join(propertyClasses, o => o.ParameterType, i => i?.GetType(), (_, i) => i).ToArray();
		
		if (args.Length != delegateArgTypes.Count())
			throw new ArgumentException($"This instance of {nameof(NamingTemplate)} requires the following arguments: {string.Join(", ", delegateArgTypes.Select(t => t.Name).Distinct())}");

		return (templateToString.DynamicInvoke(args) as TemplatePart)!.FirstPart;
	}

	/// <summary>Parse a template string to a <see cref="NamingTemplate"/></summary>
	/// <param name="template">The template string to parse</param>
	/// <param name="tagCollections">A collection of <see cref="TagCollection"/> with
	/// properties registered to match to the <paramref name="template"/></param>
	public static NamingTemplate Parse(string? template, IEnumerable<TagCollection> tagCollections)
	{
		var namingTemplate = new NamingTemplate(tagCollections);
		try
		{
			BinaryNode intermediate = namingTemplate.IntermediateParse(template);
			Expression evalTree = GetExpressionTree(intermediate);

			namingTemplate.templateToString = Expression.Lambda(evalTree, tagCollections.Select(tc => tc.Parameter)).Compile();
		}
		catch(Exception ex)
		{
			namingTemplate.errors.Add(ex.Message);
		}
		return namingTemplate;
	}

	private NamingTemplate(IEnumerable<TagCollection> properties)
	{
		TagCollections = properties;
	}

	/// <summary>Builds an <see cref="Expression"/> tree that will evaluate to a <see cref="TemplatePart"/></summary>
	private static Expression GetExpressionTree(BinaryNode? node)
	{
		if (node is null) return TemplatePart.Blank;
		else if (node.IsValue) return node.Expression;
		else if (node.IsConditional) return Expression.Condition(node.Expression, concatExpression(node), TemplatePart.Blank);
		else return concatExpression(node);

		static Expression concatExpression(BinaryNode node)
			=> TemplatePart.CreateConcatenation(GetExpressionTree(node.LeftChild), GetExpressionTree(node.RightChild));
	}

	/// <summary>Parse a template string into a <see cref="BinaryNode"/> tree</summary>
	private BinaryNode IntermediateParse(string? templateString)
	{
		if (templateString is null)
			throw new ArgumentException(ERROR_NULL_IS_INVALID);
		else if (string.IsNullOrEmpty(templateString))
			warnings.Add(WARNING_EMPTY);
		else if (string.IsNullOrWhiteSpace(templateString))
			warnings.Add(WARNING_WHITE_SPACE);

		TemplateText = templateString;

		BinaryNode topNode = BinaryNode.CreateRoot();
		BinaryNode? currentNode = topNode;
		List<char> literalChars = new();

		while (templateString.Length > 0)
		{
			if (StartsWith(templateString, out var exactPropertyName, out var propertyTag, out var valueExpression))
			{
				checkAndAddLiterals();

				if (propertyTag is IClosingPropertyTag)
					currentNode = currentNode.AddNewNode(BinaryNode.CreateConditional(propertyTag.TemplateTag, valueExpression));
				else
				{
					currentNode = currentNode.AddNewNode(BinaryNode.CreateValue(propertyTag.TemplateTag, valueExpression));
					_tagsInUse.Add(propertyTag.TemplateTag);
				}

				templateString = templateString[exactPropertyName.Length..];
			}
			else if (StartsWithClosing(templateString, out exactPropertyName, out var closingPropertyTag))
			{
				checkAndAddLiterals();

				BinaryNode? lastParenth = currentNode;

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
				currentNode = currentNode.AddNewNode(BinaryNode.CreateValue(string.Concat(literalChars)));
				literalChars.Clear();
			}
		}
	}

	private bool StartsWith(string template, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IPropertyTag? propertyTag, [NotNullWhen(true)] out Expression? valueExpression)
	{
		foreach (var pc in TagCollections)
		{
			if (pc.StartsWith(template, out exactName, out propertyTag, out valueExpression))
				return true;
		}

		exactName = null;
		valueExpression = null;
		propertyTag = null;
		return false;
	}

	private bool StartsWithClosing(string template, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IClosingPropertyTag? closingPropertyTag)
	{
		foreach (var pc in TagCollections)
		{
			if (pc.StartsWithClosing(template, out exactName, out closingPropertyTag))
				return true;
		}

		exactName = null;
		closingPropertyTag = null;
		return false;
	}

	private class BinaryNode
	{
		public string Name { get; }
		public BinaryNode? Parent { get; private set; }
		public BinaryNode? RightChild { get; private set; }
		public BinaryNode? LeftChild { get; private set; }
		public Expression Expression { get; }
		public bool IsConditional { get; private init; } = false;
		public bool IsValue { get; private init; } = false;

		public static BinaryNode CreateRoot() => new("Root", Expression.Empty());

		public static BinaryNode CreateValue(string literal)
			=> new("Literal", TemplatePart.CreateLiteral(literal))
			{
				IsValue = true
			};

		public static BinaryNode CreateValue(ITemplateTag templateTag, Expression property)
			=> new(templateTag.TagName, TemplatePart.CreateProperty(templateTag, property))
			{
				IsValue = true
			};

		public static BinaryNode CreateConditional(ITemplateTag templateTag, Expression property)
			=> new(templateTag.TagName, property)
			{
				IsConditional = true
			};

		private static BinaryNode CreateConcatenation(BinaryNode left, BinaryNode right)
		{
			var newNode = new BinaryNode("Concatenation", Expression.Empty())
			{
				LeftChild = left,
				RightChild = right
			};
			newNode.LeftChild.Parent = newNode;
			newNode.RightChild.Parent = newNode;
			return newNode;
		}

		private BinaryNode(string name, Expression expression)
		{
			Name = name;
			Expression = expression;
		}

		public override string ToString() => Name;

		public BinaryNode AddNewNode(BinaryNode newNode)
		{
			BinaryNode currentNode = this;

			if (LeftChild is null)
			{
				newNode.Parent = currentNode;
				LeftChild = newNode;
			}
			else if (RightChild is null)
			{
				newNode.Parent = currentNode;
				RightChild = newNode;
			}
			else
			{
				RightChild = CreateConcatenation(RightChild, newNode);
				RightChild.Parent = currentNode;
				currentNode = RightChild;
			}

			return newNode.IsConditional ? newNode : currentNode;
		}
	}
}
