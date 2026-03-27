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
	public IEnumerable<ITemplateTag> TagsRegistered => _tagCollections.SelectMany(t => t).DistinctBy(t => t.TagName);
	public IEnumerable<string> Warnings => _errors.Concat(_warnings);
	public IEnumerable<string> Errors => _errors;

	private Delegate? _templateToString;
	private readonly List<string> _warnings = [];
	private readonly List<string> _errors = [];
	private readonly List<TagCollection> _tagCollections;
	private readonly List<ITemplateTag> _tagsInUse = [];

	public const string ErrorNullIsInvalid = "Null template is invalid.";
	public const string WarningEmpty = "Template is empty.";
	public const string WarningWhiteSpace = "Template is white space.";
	public const string WarningNoTags = "Should use tags. Eg: <title>";

	/// <summary>
	/// Invoke the <see cref="NamingTemplate"/>
	/// </summary>
	/// <param name="propertyClasses">Instances of the TClass used in <see cref="PropertyTagCollection{TClass}"/> and <see cref="ConditionalTagCollection{TClass}"/></param>
	public TemplatePart Evaluate(params object?[] propertyClasses)
	{
		if (_templateToString is null)
			throw new InvalidOperationException();

		// Match propertyClasses to the arguments required by templateToString.DynamicInvoke(). 
		// First parameter is "this", so ignore it.
		var delegateArgTypes = _templateToString.Method.GetParameters().Skip(1).Select(p => p.ParameterType).ToList();
		var delegateArgs = new object?[delegateArgTypes.Count];

		var availableObjects = propertyClasses.Where(pc => pc is not null).Cast<object>().ToList();
		for (var i = 0; i < delegateArgTypes.Count; i++)
		{
			var p = delegateArgTypes[i];
			var index = availableObjects.FindIndex(pc => p.IsInstanceOfType(pc));
			if (index < 0)
			{
				if (CanBeNull(p))
					delegateArgs[i] = null;
				else
					throw new ArgumentException(
						$"No matching object found for parameter type {p.Name}. Available objects: {string.Join(", ", availableObjects.Select(o => o.GetType().Name))}");
			}
			else
			{
				var candidate = availableObjects[index];
				availableObjects.RemoveAt(index);
				availableObjects.Add(candidate); // Re-add to the end to allow reuse if needed later
				delegateArgs[i] = candidate;
			}
		}

		return (_templateToString.DynamicInvoke(delegateArgs) as TemplatePart)!.FirstPart;
	}
	
	/// <summary>Parse a template string to a <see cref="NamingTemplate"/></summary>
	/// <param name="template">The template string to parse</param>
	/// <param name="tagCollections">A collection of <see cref="TagCollection"/> with
	/// properties registered to match to the <paramref name="template"/></param>
	public static NamingTemplate Parse(string? template, IEnumerable<TagCollection> tagCollections)
	{
		var listOfTagCollections = tagCollections.ToList();
		var namingTemplate = new NamingTemplate(listOfTagCollections);
		try
		{
			var intermediate = namingTemplate.IntermediateParse(template);
			var evalTree = GetExpressionTree(intermediate);

			namingTemplate._templateToString = Expression.Lambda(evalTree, listOfTagCollections.Select(tc => tc.Parameter).Append(TagCollection.CultureParameter)).Compile();
		}
		catch (Exception ex)
		{
			namingTemplate._errors.Add(ex.Message);
		}
		return namingTemplate;
	}

	private NamingTemplate(List<TagCollection> properties)
	{
		_tagCollections = properties;
	}

	/// <summary>Builds an <see cref="Expression"/> tree that will evaluate to a <see cref="TemplatePart"/></summary>
	private static Expression GetExpressionTree(BinaryNode? node)
	{
		if (node is null) return TemplatePart.Blank;
		if (node.IsValue) return node.Expression;
		return node.IsConditional
			? Expression.Condition(node.Expression, ConcatExpression(node), TemplatePart.Blank)
			: ConcatExpression(node);

		static Expression ConcatExpression(BinaryNode node)
			=> TemplatePart.CreateConcatenation(GetExpressionTree(node.LeftChild), GetExpressionTree(node.RightChild));
	}

	/// <summary>Parse a template string into a <see cref="BinaryNode"/> tree</summary>
	private BinaryNode IntermediateParse(string? templateString)
	{
		if (templateString is null)
			throw new ArgumentException(ErrorNullIsInvalid);
		if (string.IsNullOrEmpty(templateString))
			_warnings.Add(WarningEmpty);
		else if (string.IsNullOrWhiteSpace(templateString))
			_warnings.Add(WarningWhiteSpace);

		TemplateText = templateString;

		var topNode = BinaryNode.CreateRoot();
		var currentNode = topNode;
		List<char> literalChars = [];

		while (templateString.Length > 0)
		{
			if (StartsWith(templateString, OutputType.String, out var exactPropertyName, out var propertyTag, out var valueExpression))
			{
				CheckAndAddLiterals();

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
				CheckAndAddLiterals();

				var lastParent = currentNode;

				while (lastParent?.IsConditional is false)
					lastParent = lastParent.Parent;

				if (lastParent?.Parent is null)
				{
					_warnings.Add($"Missing <{closingPropertyTag.TemplateTag.TagName}-> open conditional.");
					break;
				}
				else if (lastParent.Name != closingPropertyTag.TemplateTag.TagName)
				{
					_warnings.Add($"Missing <-{lastParent.Name}> closing conditional.");
					break;
				}

				currentNode = lastParent.Parent;
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

		CheckAndAddLiterals();

		//Check for any conditionals that haven't been closed
		while (currentNode is not null)
		{
			if (currentNode.IsConditional)
				_warnings.Add($"Missing <-{currentNode.Name}> closing conditional.");
			currentNode = currentNode.Parent;
		}

		if (!_tagsInUse.Any())
			_warnings.Add(WarningNoTags);

		return topNode;

		void CheckAndAddLiterals()
		{
			if (literalChars.Count != 0)
			{
				currentNode = currentNode.AddNewNode(BinaryNode.CreateValue(string.Concat(literalChars)));
				literalChars.Clear();
			}
		}
	}

	private bool StartsWith(string template, OutputType outputType, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IPropertyTag? propertyTag,
		[NotNullWhen(true)] out Expression? valueExpression)
	{
		foreach (var pc in _tagCollections)
		{
			if (pc.StartsWith(template, outputType, out exactName, out propertyTag, out valueExpression))
				return true;
		}

		exactName = null;
		valueExpression = null;
		propertyTag = null;
		return false;
	}

	private bool StartsWithClosing(string template, [NotNullWhen(true)] out string? exactName, [NotNullWhen(true)] out IClosingPropertyTag? closingPropertyTag)
	{
		foreach (var pc in _tagCollections)
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
		public bool IsConditional { get; private init; }
		public bool IsValue { get; private init; }

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
			var currentNode = this;

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

	private static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
}
