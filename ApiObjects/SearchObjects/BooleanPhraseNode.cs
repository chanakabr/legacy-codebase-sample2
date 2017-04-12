using ApiObjects.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    /// <summary>
    /// Representing the abstract class of a node in a tree-boolean phrase
    /// </summary>
    [DataContract]
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BooleanPhraseNode
    {
        #region Consts

        private const string AND_TOKEN = "(and";
        private const string OR_TOKEN = "(or";

        #endregion

        #region Static
        protected static TypeNameSerializationBinder binder;

        static BooleanPhraseNode()
        {
            binder = new TypeNameSerializationBinder("ApiObjects.SearchObjects.{0}, ApiObjects");
        }

        #endregion

        [JsonProperty()]
        public abstract BooleanNodeType type
        {
            get;
        }

        public BooleanPhraseNode()
        {
        }

        #region Parse Expression

        // returns tree representing the search expression 
        // example of expression: "(and actor='brad pitt' (or genre='drama' genre='action'))"
        // when the expression "actor='brad pitt'" represented by BooleanLeaf and the list represented by BooleanPhrase
        public static Status ParseSearchExpression(string expression, ref BooleanPhraseNode tree)
        {
            tree = null;

            if (string.IsNullOrEmpty(expression))
            {
                return new Status((int)eResponseStatus.OK, string.Empty);
            }

            List<string> tokens = null;

            Status status = GetTokensList(expression, ref tokens);

            if (status == null)
            {
                return new Status((int)eResponseStatus.Error, string.Empty);
            }

            if (status.Code != (int)eResponseStatus.OK)
            {
                return status;
            }

            if (tokens != null && tokens.Count > 0)
            {
                Stack stack = new Stack();
                object poped = null;
                BooleanPhrase booleanPhrase = null;
                BooleanLeaf booleanLeaf = null;

                foreach (var token in tokens)
                {
                    if (token == AND_TOKEN) // and operand - beginning of BooleanPhrase - add to stack as enum
                    {
                        stack.Push(eCutType.And);
                    }

                    else if (token == OR_TOKEN) // or operand - beginning of BooleanPhrase - add to stack as enum
                    {
                        stack.Push(eCutType.Or);
                    }

                    else if ("!=<=>=!~^:*".Contains(token)) // comparison operator - parse to enum and add to stack
                    {
                        ComparisonOperator comparisonOperator = GetComparisonOperator(token);
                        stack.Push(comparisonOperator);
                    }

                    else if (token == ")") // end of BooleanPhrase - build the BooleanPhrase by poping the parts from the stack and add it to the stack
                    {
                        if (stack.Count > 0)
                        {
                            booleanPhrase = new BooleanPhrase(new List<BooleanPhraseNode>());

                            poped = stack.Pop();

                            while (poped is BooleanPhraseNode && stack.Count > 0)
                            {
                                booleanPhrase.nodes.Add((BooleanPhraseNode)poped);
                                poped = stack.Pop();
                            }

                            if (poped is eCutType)
                            {
                                booleanPhrase.operand = (eCutType)poped;
                            }
                            else
                            {
                                return null;
                            }

                            stack.Push(booleanPhrase);
                        }
                        else
                        {
                            return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected token: {0}", token));
                        }
                    }
                    else // part of BooleanLeaf
                    {
                        if (stack.Count > 0 && stack.Peek() is ComparisonOperator)  // end of BooleanLeaf - build the BooleanLeaf by popping the parts from the stack and add it to the stack
                        {
                            booleanLeaf = new BooleanLeaf();
                            booleanLeaf.value = token;
                            booleanLeaf.operand = (ComparisonOperator)stack.Pop();
                            if (stack.Count > 0 && stack.Peek() is string)
                            {
                                booleanLeaf.field = (string)stack.Pop();
                            }
                            else
                            {
                                return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected token: {0}", token));
                            }

                            stack.Push(booleanLeaf);
                        }
                        else // beginning of BooleanLeaf - add to stack
                        {
                            stack.Push(token);
                        }
                    }
                }

                if (stack.Count == 1) // the stack should contain only one BooleanPhraseNode representing the full tree
                {
                    tree = (BooleanPhraseNode)stack.Pop();
                }
                else
                {
                    return new Status((int)eResponseStatus.SyntaxError, string.Format("Invalid expression structure"));
                }
            }

            return status;
        }


        // parse the comparison operator
        private static ComparisonOperator GetComparisonOperator(string token)
        {
            ComparisonOperator comparisonOperator;
            switch (token)
            {
                case "=":
                comparisonOperator = ComparisonOperator.Equals;
                break;
                case "!=":
                comparisonOperator = ComparisonOperator.NotEquals;
                break;
                case "<":
                comparisonOperator = ComparisonOperator.LessThan;
                break;
                case ">":
                comparisonOperator = ComparisonOperator.GreaterThan;
                break;
                case "<=":
                comparisonOperator = ComparisonOperator.LessThanOrEqual;
                break;
                case ">=":
                comparisonOperator = ComparisonOperator.GreaterThanOrEqual;
                break;
                case "~":
                comparisonOperator = ComparisonOperator.Contains;
                break;
                case "!~":
                comparisonOperator = ComparisonOperator.NotContains;
                break;
                case "^":
                comparisonOperator = ComparisonOperator.WordStartsWith;
                break;
                case ":":
                comparisonOperator = ComparisonOperator.In;
                break;
                case "*":
                comparisonOperator = ComparisonOperator.Phonetic;
                break;
                default:
                comparisonOperator = ComparisonOperator.Contains;
                break;
            }

            return comparisonOperator;
        }

        // returns a list of tokens when each token represents one of the following:
        //   operand - "(or" or "(and" 
        //   quote - the search value - the expression between the '' (in the example it's 'brad pitt')
        //   comparison operator - "<", ">", "=", "~", "<=", ">=", "!="
        //   end of expression with operand = ")" 
        //   word - is a tag or meta neme (in the example it's actor)
        private static Status GetTokensList(string expression, ref List<string> tokens)
        {
            Status status;

            tokens = new List<string>();

            expression = expression.Trim();

            char[] buffer = new char[expression.Length];
            int lastBufferIndex = 0;

            //for the following, true if the expression inserted to the buffer is:
            bool isQuote = false; // between '  '
            bool isOperand = false; // operand including spaces and '('
            bool isOperandWord = false; // just operand word ("and", "or") - no spaces or '(' 
            bool isWord = false; // word (like described above)

            string token = null;

            for (int i = 0; i < expression.Length; ++i)
            {
                char chr = expression[i];

                if (chr == '\'' && !isQuote) // beginning of quote - add to buffer 
                {
                    buffer[lastBufferIndex++] = chr;
                    buffer[lastBufferIndex] = '\0';
                    isQuote = true;
                }
                else if (chr == '\'' && isQuote) // end of quote - get the token from the buffer and add to tokens list (without '') 
                {
                    if (GetTokenFromBuffer("'", true, false, ref buffer, ref token))
                    {
                        lastBufferIndex = 0;
                        token = token.Replace("%27", "'");
                        tokens.Add(token);
                        isQuote = false;
                        isWord = false;
                    }
                    else
                    {
                        return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: {0} , on index {1}", chr, i));
                    }
                }
                else if (chr == ' ') // space 
                {
                    if (isQuote || isWord) // in a quote or in a word - add to buffer
                    {
                        buffer[lastBufferIndex++] = chr;
                        buffer[lastBufferIndex] = '\0';
                    }
                    else if (isOperandWord) // in operand word - meaning the end of the operand - get the operand from buffer and add to tokens list
                    {
                        if (GetTokenFromBuffer("(", true, true, ref buffer, ref token) && (token == AND_TOKEN || token == OR_TOKEN))
                        {
                            lastBufferIndex = 0;
                            tokens.Add(token);
                            isOperand = false;
                            isOperandWord = false;
                        }
                        else
                        {
                            return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: space , on index {0}", i));
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (chr == '(' && !isQuote) // beginning of operand - add to buffer
                {
                    buffer[lastBufferIndex++] = chr;
                    buffer[lastBufferIndex] = '\0';
                    isOperand = true;
                }
                // single comparison operator or end of expression with operand - 
                // get the full token from the buffer if availible and add to tokens list, add the seperator to tokens list
                else if ((chr == ')' || chr == '~' || chr == '=' || chr == '^' || chr == ':' || chr == '*') && !isQuote) 
                {
                    if (GetTokenFromBuffer(string.Empty, false, true, ref buffer, ref token))
                    {
                        isWord = false;
                        lastBufferIndex = 0;
                        if (!string.IsNullOrEmpty(token))
                        {
                            tokens.Add(token);
                        }
                        else if (chr != ')') // error - comparison operator must follow word
                        {
                            return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: {0} , on index {1}", chr, i));
                        }

                        token = new string(chr, 1);
                        tokens.Add(token);
                    }
                    else
                    {
                        return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: {0} , on index {1}", chr, i));
                    }
                }
                else if ((chr == '>' || chr == '<' || chr == '!') && !isQuote) // double or single comparison operator - get the token from buffer if availible and add to tokens list, add the seperator to tokens list 
                {
                    if (GetTokenFromBuffer(string.Empty, false, true, ref buffer, ref token))
                    {
                        lastBufferIndex = 0;
                        if (!string.IsNullOrEmpty(token))
                        {
                            tokens.Add(token);
                        }
                        else
                        {
                            return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: {0} , on index {1}", chr, i));
                        }
                    }
                    else
                    {
                        return new Status((int)eResponseStatus.SyntaxError, string.Format("Unexpected char: {0} , on index {1}", chr, i));
                    }

                    // double comparison operator - add the full operator to tokens list and skip the next char in the loop
                    if (i + 1 < expression.Length && (expression[i + 1] == '=' || expression[i + 1] == '~'))
                    {
                        token = new string(new char[2] { chr, expression[i + 1] });
                        tokens.Add(token);

                        i = i + 1; // skip the next char (already handled)
                    }
                    else // single comparison operator - add to tokens list
                    {
                        token = new string(chr, 1);
                        tokens.Add(token);
                    }

                    isWord = false;
                }
                else // any other char - add to buffer
                {
                    isWord = !isOperand; // a word which is not an operand
                    isOperandWord = isOperand; // operand but not space or '('

                    buffer[lastBufferIndex++] = chr;

                    // when the entire expression is invalid (input is a random sentence)
                    // we will get here and exceed the limit
                    if (lastBufferIndex < expression.Length)
                    {
                        buffer[lastBufferIndex] = '\0';
                    }
                    else
                    {
                        lastBufferIndex = expression.Length - 1;
                    }
                }
            }

            if (buffer[lastBufferIndex] == '\0')
            {
                status = new Status((int)eResponseStatus.OK, string.Empty);
            }
            else
            {
                status = new Status((int)eResponseStatus.SyntaxError, string.Empty);
            }

            return status;

        }

        // Returns a full token from the buffer:
        // containsCondition - a string containing chars that one of them should be at the beginning of the token 
        // shouldVerifyCondition - true if 'containsCondition' must be checked
        // shouldAppendFirst - true if the first char should be part of the token
        private static bool GetTokenFromBuffer(string containsCondition, bool shouldVerifyCondition, bool shouldAppendFirst, ref char[] buffer, ref string token)
        {
            token = null;

            if (shouldVerifyCondition && !containsCondition.Contains(buffer[0]))
            {
                return false;
            }

            if (buffer[0] != '\0')
            {
                int i = 0;

                while (buffer[i] != '\0')
                {
                    i++;
                }

                if (shouldAppendFirst)
                {
                    token = new string(buffer, 0, i);
                }
                else
                {
                    token = new string(buffer, 1, i - 1);
                }

                token = token.Trim();

                buffer[0] = '\0';
            }

            return true;
        }

        #endregion

        #region Statis Methods

        public static void ReplaceLeafWithPhrase(ref BooleanPhraseNode filterTree,
            Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping, BooleanLeaf leaf, BooleanPhraseNode newPhrase)
        {
            // If there is a parent to this leaf - remove the old leaf and add the new phrase instead of it
            if (parentMapping.ContainsKey(leaf))
            {
                parentMapping[leaf].nodes.Remove(leaf);
                parentMapping[leaf].nodes.Add(newPhrase);
            }
            else
            // If it doesn't exist in the mapping, it's probably the root
            {
                filterTree = newPhrase;
            }
        }

        #endregion

        internal static BooleanPhraseNode Deserialize(string value)
        {
            BooleanPhraseNode result = null;
            JObject jObject = JObject.Parse(value);

            result = JsonConvert.DeserializeObject<BooleanPhraseNode>(value,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Binder = binder
                });

            return result;
        }

        internal static string Serialize(BooleanPhraseNode value)
        {
            string json = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Binder = binder
            });

            return json;
        }
    }

    public enum BooleanNodeType
    {
        Leaf,
        Parent
    }

    public class TypeNameSerializationBinder : SerializationBinder
    {
        public string TypeFormat
        {
            get;
            private set;
        }

        public TypeNameSerializationBinder(string typeFormat)
        {
            TypeFormat = typeFormat;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            string resolvedTypeName = string.Format(TypeFormat, typeName);

            return Type.GetType(resolvedTypeName, true);
        }
    }
}
