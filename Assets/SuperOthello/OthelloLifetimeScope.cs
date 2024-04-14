using System.Collections;
using System.Collections.Generic;
using MessagePipe;
using SuperOthello.Model;
using VContainer;
using VContainer.Unity;

namespace SuperOthello
{
    public class OthelloLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();

            builder.RegisterMessageBroker<CellPosition>(options);
            builder.RegisterMessageBroker<CellState[,]>(options);
            builder.RegisterMessageBroker<IEnumerable<(int row, int column)>>(options);
            
            builder.Register<OthelloLogic>(Lifetime.Singleton);
            builder.RegisterEntryPoint<OthelloLogic>();
        }
    }
}
