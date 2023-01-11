// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.PowerShell.EditorServices.Services;
using Microsoft.PowerShell.EditorServices.Services.Symbols;
using Microsoft.PowerShell.EditorServices.Services.TextDocument;
using Microsoft.PowerShell.EditorServices.Test.Shared;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit;

namespace PowerShellEditorServices.Test.Services.Symbols
{
    [Trait("Category", "AstOperations")]
    public class AstOperationsTests
    {
        private readonly ScriptFile scriptFile;

        public AstOperationsTests()
        {
            WorkspaceService workspace = new(NullLoggerFactory.Instance);
            scriptFile = workspace.GetFile(TestUtilities.GetSharedPath("References/FunctionReference.ps1"));
        }

        [Theory]
        [InlineData(1, 15, "BasicFunction")]
        [InlineData(2, 3, "BasicFunction")]
        [InlineData(4, 31, "FunctionWithExtraSpace")]
        [InlineData(7, 18, "FunctionWithExtraSpace")]
        [InlineData(12, 22, "FunctionNameOnDifferentLine")]
        [InlineData(22, 13, "FunctionNameOnDifferentLine")]
        [InlineData(24, 30, "IndentedFunction")]
        [InlineData(24, 52, "IndentedFunction")]
        public void CanFindSymbolAtPosition(int lineNumber, int columnNumber, string expectedName)
        {
            SymbolReference reference = AstOperations.FindSymbolAtPosition(
                scriptFile.ScriptAst,
                lineNumber,
                columnNumber);
            Assert.NotNull(reference);
            Assert.Equal(expectedName, reference.SymbolName);
        }

        [Theory]
        [MemberData(nameof(FindReferencesOfSymbolAtPositionData))]
        public void CanFindReferencesOfSymbolAtPosition(int lineNumber, int columnNumber, Range[] symbolRange)
        {
            SymbolReference symbol = AstOperations.FindSymbolAtPosition(
                scriptFile.ScriptAst,
                lineNumber,
                columnNumber);

            Assert.True(scriptFile.References.TryGetReferences(
                symbol.SymbolName,
                out ConcurrentBag<SymbolReference> references));

            int positionsIndex = 0;
            foreach (SymbolReference reference in references.OrderBy((i) => i.ScriptRegion.StartOffset))
            {
                Assert.Equal(symbolRange[positionsIndex].Start.Line, reference.ScriptRegion.StartLineNumber);
                Assert.Equal(symbolRange[positionsIndex].Start.Character, reference.ScriptRegion.StartColumnNumber);
                Assert.Equal(symbolRange[positionsIndex].End.Line, reference.ScriptRegion.EndLineNumber);
                Assert.Equal(symbolRange[positionsIndex].End.Character, reference.ScriptRegion.EndColumnNumber);

                positionsIndex++;
            }
        }

        public static object[][] FindReferencesOfSymbolAtPositionData { get; } = new object[][]
        {
            new object[] { 1, 15, new[] { new Range(1, 10, 1, 23), new Range(2, 1, 2, 14) } },
            new object[] { 2, 3, new[] { new Range(1, 10, 1, 23), new Range(2, 1, 2, 14) } },
            new object[] { 4, 31, new[] { new Range(4, 19, 4, 41), new Range(7, 3, 7, 25) } },
            new object[] { 7, 18, new[] { new Range(4, 19, 4, 41), new Range(7, 3, 7, 25) } },
            new object[] { 22, 13, new[] { new Range(12, 8, 12, 35), new Range(22, 5, 22, 32) } },
            new object[] { 12, 22, new[] { new Range(12, 8, 12, 35), new Range(22, 5, 22, 32) } },
            new object[] { 24, 30, new[] { new Range(24, 22, 24, 38), new Range(24, 44, 24, 60) } },
            new object[] { 24, 52, new[] { new Range(24, 22, 24, 38), new Range(24, 44, 24, 60) } },
        };
    }
}
