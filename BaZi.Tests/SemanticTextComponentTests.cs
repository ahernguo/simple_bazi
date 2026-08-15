using System.Runtime.ExceptionServices;
using System.Text;
using BaZi.Components.Common;
using BaZi.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BaZi.Tests {

    public sealed class SemanticTextComponentTests {
        [Fact]
        public async Task Render_ElementAndTenGodSegments_UsesActualSegmentText() {
            var services = new ServiceCollection();
            services.AddSingleton<SemanticTextService>();
            services.AddSingleton<PeriodFavorabilityService>();
            services.AddSingleton<TenGodPresentationService>();

            await using ServiceProvider serviceProvider = services.BuildServiceProvider();
            await using var renderer = new TextCaptureRenderer(serviceProvider);
            var parameters = ParameterView.FromDictionary(
                new Dictionary<string, object?> {
                    [nameof(SemanticText.Text)] = "木日主的食神"
                }
            );

            await renderer.RenderAsync<SemanticText>(parameters);

            Assert.Contains("木日主的食神", renderer.RenderedText);
            Assert.DoesNotContain("segment.Text", renderer.RenderedText);
        }

#pragma warning disable BL0006 // 測試必須直接驅動 Renderer，才能驗證實際傳入子元件的文字參數。
        private sealed class TextCaptureRenderer(IServiceProvider serviceProvider)
            : Renderer(serviceProvider, NullLoggerFactory.Instance) {
            private readonly StringBuilder _renderedText = new();

            public override Microsoft.AspNetCore.Components.Dispatcher Dispatcher { get; } =
                Microsoft.AspNetCore.Components.Dispatcher.CreateDefault();

            public string RenderedText => _renderedText.ToString();

            public Task RenderAsync<TComponent>(ParameterView parameters)
                where TComponent : IComponent {
                return Dispatcher.InvokeAsync(async () => {
                    IComponent component = InstantiateComponent(typeof(TComponent));
                    int componentId = AssignRootComponentId(component);
                    await RenderRootComponentAsync(componentId, parameters);
                });
            }

            protected override void HandleException(Exception exception) {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) {
                ArrayRange<RenderTreeFrame> frames = renderBatch.ReferenceFrames;
                for (var index = 0; index < frames.Count; index++) {
                    RenderTreeFrame frame = frames.Array[index];
                    if (frame.FrameType == RenderTreeFrameType.Text) {
                        _renderedText.Append(frame.TextContent);
                    } else if (frame.FrameType == RenderTreeFrameType.Markup) {
                        _renderedText.Append(frame.MarkupContent);
                    }
                }

                return Task.CompletedTask;
            }
        }
#pragma warning restore BL0006
    }
}
