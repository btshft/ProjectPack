using System.Threading;

namespace PackProject.Tool.Services.ExecutionContext
{
    public class ExecutionContextAccessor : IExecutionContextAccessor
    {
        private static readonly AsyncLocal<ContextHolder> CurrentContext
            = new AsyncLocal<ContextHolder>();

        /// <inheritdoc />
        public ExecutionContext Context
        {
            get => CurrentContext.Value?.Context;
            set
            {
                var holder = CurrentContext.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    CurrentContext.Value = new ContextHolder
                    {
                        Context = value
                    };
                }
            }
        }

        private class ContextHolder
        {
            public ExecutionContext Context;
        }
    }
}