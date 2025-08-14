using System;
using System.Collections.Generic;

namespace BuildingGen.Components
{
    public class EvaluationContext
    {
        private readonly Dictionary<string, Parameter> _parameters = new();

        private readonly EvaluationContext _topContext;

        private Stack<string> _evaluationStack = new();


        public EvaluationContext(EvaluationContext topContext, Dictionary<string, Parameter> parameters)
        {
            _topContext = topContext;
            if (parameters != null)
            {
                foreach (var (name, parameter) in parameters)
                {
                    _parameters.Add(name, parameter);
                }
            }
        }

        public Parameter GetParameter(string name)
        {
            if (_parameters.TryGetValue(name, out var param))
            {
                return param;
            }
            else if (_topContext != null)
            {
                return _topContext.GetParameter(name);
            }
            else
            {
                throw new KeyNotFoundException($"Parameter '{name}' not found.");
            }
        }

        public void PushParameter(string name)
        {
            if (!_evaluationStack.Contains(name))
            {
                _evaluationStack.Push(name);
            }
            else
            {
                throw new InvalidOperationException($"Circular reference detected for parameter '{name}'.");
            }
        }

        public void PopParameter(string name)
        {
            string lastParam = _evaluationStack.Pop();
            if (lastParam != name)
            {
                throw new InvalidOperationException($"Unexpected parameter '{name}' on stack.");
            }
        }
    }
}
