﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis;
using ICSharpCode.NRefactory6.CSharp;
using System.Linq;
using ICSharpCode.NRefactory6.CSharp.GenerateMember.GenerateConstructor;
using System;

namespace MonoDevelop.CSharp.CodeFixes.GenerateConstructor
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = PredefinedCodeFixProviderNames.GenerateConstructor), Shared]
	[ExtensionOrder(After = PredefinedCodeFixProviderNames.FullyQualify)]
	internal class GenerateConstructorCodeFixProvider : AbstractGenerateMemberCodeFixProvider
	{
		private const string CS0122 = "CS0122"; // CS0122: 'C' is inaccessible due to its protection level
		private const string CS1729 = "CS1729"; // CS1729: 'C' does not contain a constructor that takes n arguments
		private const string CS1739 = "CS1739"; // CS1739: The best overload for 'Program' does not have a parameter named 'v'
		private const string CS1503 = "CS1503"; // CS1503: Argument 1: cannot convert from 'T1' to 'T2'
		private const string CS7036 = "CS7036"; // CS7036: There is no argument given that corresponds to the required formal parameter 'v' of 'C.C(int)'

		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(CS0122, CS1729, CS1739, CS1503, CS7036); }
		}

		protected override Task<IEnumerable<CodeAction>> GetCodeActionsAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
		{
			return new CSharpGenerateConstructorService ().GenerateConstructorAsync (document, node, cancellationToken);
		}

		protected override bool IsCandidate(SyntaxNode node)
		{
			return node is SimpleNameSyntax || node is ObjectCreationExpressionSyntax || node is ConstructorInitializerSyntax || node is AttributeSyntax;
		}

		protected override SyntaxNode GetTargetNode(SyntaxNode node)
		{
			var objectCreationNode = node as ObjectCreationExpressionSyntax;
			if (objectCreationNode != null)
			{
				return objectCreationNode.Type.GetRightmostName();
			}

			var attributeNode = node as AttributeSyntax;
			if (attributeNode != null)
			{
				return attributeNode.Name;
			}

			return node;
		}
	}
}
