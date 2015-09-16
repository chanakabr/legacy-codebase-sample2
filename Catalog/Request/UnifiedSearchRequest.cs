using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;

namespace Catalog.Request
{
    /// <summary>
    /// A search request of several types of assets: Media, EPGs etc. All in one, unified place.
    /// </summary>
    [DataContract]
    public class UnifiedSearchRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Members

        private const string AND_TOKEN = "(and";
        private const string OR_TOKEN = "(or";

        #endregion

        #region Data Members

        [DataMember]
        public OrderObj order;

        [DataMember]
        public List<int> assetTypes;

        internal BooleanPhraseNode filterTree;

        [DataMember]
        public string filterQuery;

        [DataMember]
        public string nameAndDescription;

        [DataMember]
        public List<ePersonalFilter> personalFilters;

        #endregion

        #region Ctor

        /// <summary>
        /// Regulat constructor that initializes the request members
        /// </summary>
        /// <param name="nPageSize"></param>
        /// <param name="nPageIndex"></param>
        /// <param name="nGroupID"></param>
        /// <param name="sSignature"></param>
        /// <param name="sSignString"></param>
        /// <param name="order"></param>
        /// <param name="searchValue"></param>
        /// <param name="ands"></param>
        /// <param name="ors"></param>
        /// <param name="type"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public UnifiedSearchRequest(int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString,
            OrderObj order,
            List<int> types,
            string filterQuery,
            string nameAndDescription,
            BooleanPhraseNode filterTree = null)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            this.order = order;
            this.assetTypes = types;
            this.filterTree = filterTree;
            this.filterQuery = filterQuery;
            this.nameAndDescription = nameAndDescription;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Perform the unified search and return the Ids of the assets and their types
        /// </summary>
        /// <param name="baseRequest"></param>
        /// <returns></returns>
        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            UnifiedSearchResponse response = new UnifiedSearchResponse();

            try
            {
                UnifiedSearchRequest request = baseRequest as UnifiedSearchRequest;

                if (request == null)
                {
                    throw new ArgumentNullException("request object is null or Required variables is null");
                }

                if (request.m_nGroupID == 0)
                {
                    var exception = new ArgumentException("No group Id was sent in request");
                    exception.Data["StatusCode"] = (int)eResponseStatus.BadSearchRequest;

                    throw exception;
                }

                BooleanPhraseNode filterTree = null;

                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    Status status = ParseSearchExpression(filterQuery, ref filterTree);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        return new UnifiedSearchResponse()
                        {
                            status = status
                        };
                    }
                }

                // If request asks for name and description filter
                if (string.IsNullOrEmpty(request.nameAndDescription))
                {
                    request.filterTree = filterTree;
                }
                else
                {
                    List<BooleanPhraseNode> newNodes = new List<BooleanPhraseNode>();
                    List<BooleanPhraseNode> nameAndDescriptionNodes = new List<BooleanPhraseNode>();

                    // "name = q OR description = q"
                    nameAndDescriptionNodes.Add(new BooleanLeaf("name", request.nameAndDescription, null, ComparisonOperator.Contains));
                    nameAndDescriptionNodes.Add(new BooleanLeaf("description", request.nameAndDescription, null, ComparisonOperator.Contains));

                    BooleanPhrase nameAndDescriptionPhrase = new BooleanPhrase(nameAndDescriptionNodes, eCutType.Or);

                    newNodes.Add(nameAndDescriptionPhrase);

                    // If there is no filter tree from the string, create a new one containing only name and description
                    // If there is a tree already, use it as a branch and connect it with "And" to "name and description" 
                    if (filterTree != null)
                    {
                        newNodes.Add(filterTree);
                    }

                    request.filterTree = new BooleanPhrase(newNodes, eCutType.And);
                }

                CheckSignature(baseRequest);

                int totalItems = 0;
                List<UnifiedSearchResult> assetsResults = Catalog.GetAssetIdFromSearcher(request, ref totalItems);

                response.m_nTotalItems = totalItems;

                if (totalItems > 0)
                {
                    response.searchResults = assetsResults;
                }

                response.status.Code = (int)eResponseStatus.OK;
            }
            catch (Exception ex)
            {
                log.Error("Error - GetResponse - " +
                    string.Format("Exception: group = {0} siteGuid = {1} filterPhrase = {2} message = {3}, ST = {4}",
                    baseRequest.m_nGroupID, // {0}
                    baseRequest.m_sSiteGuid, // {1}
                    // Use filter query if this is correct type
                    baseRequest is UnifiedSearchRequest ? (baseRequest as UnifiedSearchRequest).filterQuery : "", // {2}
                    ex.Message, // {3}
                    ex.StackTrace // {4}
                    ), ex);

                if (ex is HttpException)
                {
                    if ((ex as HttpException).GetHttpCode() == 404)
                    {
                        response.status.Code = (int)eResponseStatus.IndexMissing;
                        response.status.Message = "Data not index for this group";
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Got error with Elasticsearch";
                    }
                }
                else if (ex is ArgumentException)
                {
                    // This is a specific exception we created.
                    // If this specific ArgumentException has StatusCode in its data, use it instead of the general code
                    if (ex.Data.Contains("StatusCode"))
                    {
                        response.status.Code = (int)ex.Data["StatusCode"];
                        response.status.Message = ex.Message;
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Search failed";
                    }
                }
                else
                {
                    response.status.Code = (int)eResponseStatus.Error;
                    response.status.Message = "Search failed";
                }
            }

            return (BaseResponse)response;
        }

        #endregion

        #region Parse Expression

        // returns tree representing the search expression 
        // example of expression: "(and actor='brad pitt' (or genre='drama' genre='action'))"
        // when the expression "actor='brad pitt'" represented by BooleanLeaf and the list represented by BooleanPhrase
        internal Status ParseSearchExpression(string expression, ref BooleanPhraseNode tree)
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

                    else if ("!=<=>=!~^:".Contains(token)) // comparison operator - parse to enum and add to stack
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
        private ComparisonOperator GetComparisonOperator(string token)
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
        private Status GetTokensList(string expression, ref List<string> tokens)
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
                else if ((chr == ')' || chr == '~' || chr == '=' || chr == '^' || chr == ':') && !isQuote) // single comparison operator or end of expression with operand - get the full token from the buffer if availible and add to tokens list, add the seperator to tokens list
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
                    buffer[lastBufferIndex] = '\0';
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
        private bool GetTokenFromBuffer(string containsCondition, bool shouldVerifyCondition, bool shouldAppendFirst, ref char[] buffer, ref string token)
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
    }
}