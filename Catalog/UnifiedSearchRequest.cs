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

namespace Catalog
{
    /// <summary>
    /// A search request of several types of assets: Media, EPGs etc. All in one, unified place.
    /// </summary>
    [DataContract]
    public class UnifiedSearchRequest : BaseRequest, IRequestImp
    {
        #region Data Members

        [DataMember]
        public bool isExact;
        
        [DataMember]
        public OrderObj order;

        [DataMember]
        public List<int> assetTypes;

        [DataMember]
        internal BooleanPhraseNode filterTree;

        [DataMember]
        public string filterQuery;

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
        /// <param name="isExact"></param>
        /// <param name="order"></param>
        /// <param name="searchValue"></param>
        /// <param name="ands"></param>
        /// <param name="ors"></param>
        /// <param name="type"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public UnifiedSearchRequest(int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString,
            bool isExact, OrderObj order, string searchValue,
            BooleanPhraseNode filterTree,
            List<int> types)
                : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            this.isExact = isExact;
            this.order = order;
            this.assetTypes = types;
            this.filterTree = filterTree;
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

                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    filterTree = ParseSearchExpression(filterQuery);
                }

                // Request is bad if there is no condition to query by; or the group is 0
                else if (filterTree == null ||
                       request.m_nGroupID == 0)
                {
                    response.status.Code = (int)eResponseStatus.BadSearchRequest;
                    response.status.Message = "Invalid request parameters";
                }
                else
                {

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
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error - GetResponse", string.Format("Exception: message = {0}, ST = {1}", ex.Message, ex.StackTrace), this.GetType().Name);

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
                else
                {
                    response.status.Code = (int)eResponseStatus.Error;
                    response.status.Message = "Search failed";
                }
            }

            return (BaseResponse)response;
        }

        #endregion

        internal BooleanPhraseNode ParseSearchExpression(string expression)
        {
            BooleanPhraseNode phrase = null;

            if (string.IsNullOrEmpty(expression))
            {
                return null;
            }

            List<string> tokens = null;

            if (!GetTokensList(expression, ref tokens))
            {
                return null;
            }

            if (tokens != null && tokens.Count > 0)
            {
                Stack stack = new Stack();
                object poped = null;
                BooleanPhrase booleanPhrase = null;
                BooleanLeaf booleanLeaf = null;

                foreach (var token in tokens)
                {
                    if (token == "(and") // and opperand - beginning of BooleanPhrase
                    {
                        stack.Push(eCutType.And);
                    }

                    else if (token == "(or") // or opperand - beginning of BooleanPhrase
                    {
                        stack.Push(eCutType.Or);
                    }

                    else if ("~!=<=>=".Contains(token)) // comparison operator
                    {
                        ComparisonOperator comparisonOperator = GetComparisonOperator(token);
                        stack.Push(comparisonOperator);
                    }

                    else if (token == ")") // end of BooleanPhrase
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
                            //else error

                            stack.Push(booleanPhrase);
                        }
                        // else error
                    }
                    else // part of BooleanLeaf
                    {
                        if (stack.Count > 0 && stack.Peek() is ComparisonOperator)  // end of BooleanLeaf
                        {
                            booleanLeaf = new BooleanLeaf();
                            booleanLeaf.value = token;
                            booleanLeaf.operand = (ComparisonOperator)stack.Pop();
                            if (stack.Count > 0 && stack.Peek() is string)
                            {
                                booleanLeaf.field = (string)stack.Pop();
                            }
                            // else error

                            stack.Push(booleanLeaf);
                        }
                        else // beginning of BooleanLeaf
                        {
                            stack.Push(token);
                        }
                    }
                }

                if (stack.Count == 1)
                {
                    phrase = (BooleanPhraseNode)stack.Pop();
                }
                // else error
            }

            return phrase;
        }

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
                default:
                    comparisonOperator = ComparisonOperator.Contains;
                    break;
            }

            return comparisonOperator;
        }

        private bool GetTokensList(string expression, ref List<string> tokens)
        {
            tokens = new List<string>();    

            expression = expression.Trim();

            Stack<char> stack = new Stack<char>();
            
            bool isQuote = false;
            bool isOpperand = false;
            string token = null;

            for (int i = 0; i < expression.Length; ++i)
            {
                char chr = expression[i];

                if (chr == '\'' && !isQuote) // beginning of quote - push 
                {
                    stack.Push(chr);
                    isQuote = true;
                }

                else if (chr == '\'' && isQuote) // end of quote - pop the token and add to tokens list (without '') 
                {
                    if (PopLoop("'", true, false, ref stack, ref token))
                    {
                        tokens.Add(token);
                        isQuote = false;
                    }
                    else
                    {
                        return false;
                    }
                }

                else if (chr == ' ' && isQuote) // space in a quote - push
                {
                    stack.Push(chr);
                }

                else if (chr == ' ' && !isQuote) // space - do nothing
                {
                    continue;
                }

                else if (chr == '(') // beginning of opperand
                {
                    stack.Push(chr);
                    isOpperand = true;
                }

                else if ((chr == 'd' || chr == 'r') && isOpperand)
                {
                    stack.Push(chr);

                    if (PopLoop("(", true, true, ref stack, ref token) && (token == "(and" || token == "(or"))
                    {
                        tokens.Add(token);
                        isOpperand = false;
                    }
                    else
                    {
                        return false;
                    }
                }

                else if (chr == ')' || chr == '~' || chr == '=') // single seperator - get the pushed token if availible and add to tokens list, add the seperator to tokens list
                {
                    if (PopLoop(string.Empty, false, true, ref stack, ref token))
                    {
                        if (!string.IsNullOrEmpty(token))
                        {
                            tokens.Add(token);
                        }

                        token = new string(chr, 1);
                        tokens.Add(token);
                    }
                    else
                    {
                        return false;
                    }
                }

                else if (chr == '>' || chr == '<' || chr == '!') // double seperator - get the pushed token if availible and add to tokens list, add the seperator to tokens list 
                {
                    if (PopLoop(string.Empty, false, true, ref stack, ref token))
                    {
                        if (!string.IsNullOrEmpty(token))
                        {
                            tokens.Add(token);
                        }
                        // else error ? 

                    }
                    else
                    {
                        return false;
                    }

                    if (i + 1 < expression.Length && expression[i + 1] == '=')
                    {
                        token = new string(new char[2] { chr, expression[i + 1] });
                        tokens.Add(token);

                        i = i + 1; // skip the next char (already handled)
                    }
                    else
                    {
                        token = new string(chr, 1);
                        tokens.Add(token);
                    }
                }

                else // any other char - push
                {
                    stack.Push(chr);
                }
            }

            if (stack.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // pops tokens from the stack:
        // containsCondition - a string containing the chars which are the condition to stop the loop
        // shouldVarifyCondition - true if the reason to stop the loop must be one of the chars in 'containsCondition', false if an empty stack is enough
        // shouldAppendLast - true if the last poped char should be appended to the returned token

        private bool PopLoop(string containsCondition, bool shouldVarifyCondition, bool shouldAppendLast, ref Stack<char> stack, ref string token)
        {
            token = null;
            if (stack.Count > 0)
            {
                StringBuilder sbToken = new StringBuilder();
                char popedChar = stack.Pop();
                while (!containsCondition.Contains(popedChar) && stack.Count > 0)
                {
                    sbToken.Append(popedChar);
                    popedChar = stack.Pop();
                }

                if (shouldVarifyCondition && !containsCondition.Contains(popedChar))
                {
                    return false;
                }

                if (shouldAppendLast)
                {
                    sbToken.Append(popedChar);
                }

                token = sbToken.ToString();
                char[] charArr = token.ToCharArray();
                Array.Reverse(charArr);
                token = new string(charArr);

                return true;
            }
            
            if (shouldVarifyCondition)
                return false;
            
            return true;
        }
        
    }
}