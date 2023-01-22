using System;
using System.Collections.Generic;
using System.Text;

namespace TVinciShared
{
    public class KsqlBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public KsqlBuilder Equal<T>(string field, T value)
        {
            Op(field, "=", value);
            return this;
        }

        public KsqlBuilder Greater<T>(string field, T value)
        {
            Op(field, ">", value);
            return this;
        }

        public KsqlBuilder NotExists(string field)
        {
            Op(field, "!+", string.Empty);
            return this;
        }

        public KsqlBuilder Values<T>(Func<string, T, KsqlBuilder> fn, string field, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                fn(field, value);
            }

            return this;
        }

        private KsqlBuilder Op<T>(string field, string op, T value)
        {
            AppendSpaceIfRequired();
            _builder.Append(field).Append(op).Append("'").Append(value).Append("'");

            return this;
        }

        public KsqlBuilder Or(Action<KsqlBuilder> action)
        {
            AppendSpaceIfRequired();
            _builder.Append("(or");
            action(this);
            _builder.Append(")");

            return this;
        }

        public KsqlBuilder And(Action<KsqlBuilder> action)
        {
            AppendSpaceIfRequired();
            _builder.Append("(and");
            action(this);
            _builder.Append(")");

            return this;
        }

        private void AppendSpaceIfRequired()
        {
            if (_builder.Length != 0 && _builder[_builder.Length - 1] != ' ')
            {
                _builder.Append(' ');
            }
        }

        public string Build() => _builder.ToString();
    }
}