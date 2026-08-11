using System.Runtime.ExceptionServices;
using BaZi.Components.Fortune;
using BaZi.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BaZi.Tests {

    public class TaiSuiAnalysisSectionTests {
        [Fact]
        public async Task Render_DuplicateInteractionTypesAcrossPillars_CanRenderTwice() {
            var analysis = new TaiSuiAnalysisResult(
                2026,
                TianGan.Bing,
                DiZhi.Wu,
                "馬",
                "蛇",
                DiZhi.Si,
                [],
                [
                    new TaiSuiPillarInteraction("月柱", DiZhi.Chou, [TaiSuiInteractionType.SixHarm]),
                    new TaiSuiPillarInteraction("日柱", DiZhi.Chou, [TaiSuiInteractionType.SixHarm])
                ],
                true
            );
            var parameters = ParameterView.FromDictionary(
                new Dictionary<string, object?> {
                    [nameof(TaiSuiAnalysisSection.Analysis)] = analysis
                }
            );
            await using var renderer = new ComponentTestRenderer();

            await renderer.RenderTwiceAsync(new TaiSuiAnalysisSection(), parameters);
        }

#pragma warning disable BL0006 // 測試必須直接驅動 Renderer，才能驗證第二次差異轉譯的 @key 唯一性。
        private sealed class ComponentTestRenderer : Renderer {
            public override Microsoft.AspNetCore.Components.Dispatcher Dispatcher { get; } =
                Microsoft.AspNetCore.Components.Dispatcher.CreateDefault();

            public ComponentTestRenderer()
                : base(
                    new ServiceCollection().BuildServiceProvider(),
                    NullLoggerFactory.Instance
                ) {
            }

            public Task RenderTwiceAsync(IComponent component, ParameterView parameters) {
                return Dispatcher.InvokeAsync(async () => {
                    int componentId = AssignRootComponentId(component);
                    await RenderRootComponentAsync(componentId, parameters);
                    await RenderRootComponentAsync(componentId, parameters);
                });
            }

            protected override void HandleException(Exception exception) {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) {
                return Task.CompletedTask;
            }
        }
#pragma warning restore BL0006
    }
}
