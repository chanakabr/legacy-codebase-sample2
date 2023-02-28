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

        public KsqlBuilder In(string field, IEnumerable<long> values)
        {
            Op(field, ":", string.Join(",", values));

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

        private void Op<T>(string field, string op, T value)
        {
            AppendSpaceIfRequired();
            _builder.Append(field).Append(op).Append("'").Append(value).Append("'");
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

        public KsqlBuilder RawKSql(string kSql)
        {
            AppendSpaceIfRequired();
            _builder.Append(kSql);

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