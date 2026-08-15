using BaZi.Components.Pages;
using BaZi.Models;
using BaZi.Services;
using BaZi.Store;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

#pragma warning disable BL0006 // TestRenderer intentionally exercises Blazor's event dispatch pipeline.

namespace BaZi.Tests {

    public class DailyFortuneComponentTests {
        [Fact]
        public async Task SelectMovingTopicAndQuery_DoesNotThrow() {
            var birthDate = new DateTime(1990, 1, 1, 12, 0, 0);
            var info = new BaZiInfo(birthDate, 2);
            var state = new BaZiState(false, true, null, birthDate, 2, info);
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddFluxor(options => options.ScanAssemblies(typeof(BaZiState).Assembly));
            services.AddSingleton<IState<BaZiState>>(new TestState<BaZiState>(state));
            services.AddSingleton<BaZiService>();
            services.AddSingleton<FortuneService>();
            services.AddSingleton<DailyFortuneService>();
            services.AddSingleton<PeriodFavorabilityService>();
            services.AddSingleton<TenGodPresentationService>();
            services.AddSingleton<EarthlyBranchRelationshipEngine>();
            services.AddSingleton<NavigationManager, TestNavigationManager>();

            await using var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            await using var renderer = new TestRenderer(serviceProvider, loggerFactory);

            await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<DailyFortune>());
            var queryClickHandler = renderer.GetEventHandler("daily-query", "onclick");
            var topicChangeHandler = renderer.GetEventHandler("daily-topic", "onchange");
            await renderer.Dispatcher.InvokeAsync(() => renderer.DispatchEventAsync(
                topicChangeHandler,
                null,
                new ChangeEventArgs { Value = nameof(DailyFortuneTopic.Moving) }
            ));
            var firstHouseholdChangeHandler = renderer.GetFirstEventHandler("household-", "onchange");
            await renderer.Dispatcher.InvokeAsync(() => renderer.DispatchEventAsync(
                firstHouseholdChangeHandler,
                null,
                new ChangeEventArgs { Value = true }
            ));
            await renderer.Dispatcher.InvokeAsync(() => renderer.DispatchEventAsync(
                queryClickHandler,
                null,
                new MouseEventArgs()
            ));

            Assert.Null(renderer.UnhandledException);
        }

        private sealed class TestState<TState>(TState value) : IState<TState> {
            public event EventHandler? StateChanged {
                add { }
                remove { }
            }

            public TState Value { get; } = value;
        }

        private sealed class TestNavigationManager : NavigationManager {
            public TestNavigationManager() {
                Initialize("https://localhost/", "https://localhost/daily-fortune");
            }
        }

        private sealed class TestRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : Renderer(serviceProvider, loggerFactory) {
            private readonly Dictionary<string, Dictionary<string, ulong>> _eventHandlersByElement = [];

            public override Microsoft.AspNetCore.Components.Dispatcher Dispatcher { get; } =
                Microsoft.AspNetCore.Components.Dispatcher.CreateDefault();

            public Exception? UnhandledException { get; private set; }

            public async Task RenderComponentAsync<TComponent>() where TComponent : IComponent {
                var component = InstantiateComponent(typeof(TComponent));
                var componentId = AssignRootComponentId(component);
                await RenderRootComponentAsync(componentId, ParameterView.Empty);
            }

            public ulong GetEventHandler(string elementId, string attributeName) {
                return _eventHandlersByElement[elementId][attributeName];
            }

            public ulong GetFirstEventHandler(string elementIdPrefix, string attributeName) {
                return _eventHandlersByElement
                    .First(pair => pair.Key.StartsWith(elementIdPrefix, StringComparison.Ordinal))
                    .Value[attributeName];
            }

            protected override void HandleException(Exception exception) {
                UnhandledException = exception;
            }

            protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) {
                var frames = renderBatch.ReferenceFrames;
                for (var index = 0; index < frames.Count; index++) {
                    var frame = frames.Array[index];
                    if (frame.FrameType != RenderTreeFrameType.Element) {
                        continue;
                    }

                    string? elementId = null;
                    var eventHandlers = new Dictionary<string, ulong>();
                    var subtreeEnd = index + frame.ElementSubtreeLength;
                    for (var attributeIndex = index + 1; attributeIndex < subtreeEnd; attributeIndex++) {
                        var attribute = frames.Array[attributeIndex];
                        if (attribute.FrameType != RenderTreeFrameType.Attribute) {
                            break;
                        }

                        if (attribute.AttributeName == "id") {
                            elementId = attribute.AttributeValue?.ToString();
                        } else if (attribute.AttributeEventHandlerId != 0) {
                            eventHandlers[attribute.AttributeName] = attribute.AttributeEventHandlerId;
                        }
                    }

                    if (elementId is not null && eventHandlers.Count > 0) {
                        _eventHandlersByElement[elementId] = eventHandlers;
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}

#pragma warning restore BL0006
