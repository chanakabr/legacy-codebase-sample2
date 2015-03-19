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

                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    filterTree = ParseSearchExpression(filterQuery);
                }

                // If request asks for name and description filter
                if (!string.IsNullOrEmpty(request.nameAndDescription))
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

                    filterTree = new BooleanPhrase(newNodes, eCutType.And);
                }

                // Request is bad if there is no condition to query by; or the group is 0
                if (filterTree == null || request.m_nGroupID == 0)
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
                if (ex is ArgumentException)
                {
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
                    if (token == AND_TOKEN) // and operand - beginning of BooleanPhrase
                    {
                        stack.Push(eCutType.And);
                    }

                    else if (token == OR_TOKEN) // or operand - beginning of BooleanPhrase
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

            char[] buffer = new char[expression.Length];
            int lastBufferIndex = 0;

            bool isQuote = false;
            bool isOperand = false; // operand including spaces and '('
            bool isOperandWord = false; // just operand word - no spaces or '('
            bool isWord = false;

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
                        tokens.Add(token);
                        isQuote = false;
                        isWord = false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (chr == ' ') // space in a quote - add to buffer
                {
                    if (isQuote || isWord)
                    {
                        buffer[lastBufferIndex++] = chr;
                        buffer[lastBufferIndex] = '\0';
                    }
                    else if (isOperandWord)
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
                            return false;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (chr == '(') // beginning of operand
                {
                    buffer[lastBufferIndex++] = chr;
                    buffer[lastBufferIndex] = '\0';
                    isOperand = true;
                }
                else if (chr == ')' || chr == '~' || chr == '=') // single seperator - get the full token from the buffer if availible and add to tokens list, add the seperator to tokens list
                {
                    if (GetTokenFromBuffer(string.Empty, false, true, ref buffer, ref token))
                    {
                        isWord = false;
                        lastBufferIndex = 0;
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
                else if (chr == '>' || chr == '<' || chr == '!') // double seperator - get the token from buffer if availible and add to tokens list, add the seperator to tokens list 
                {
                    if (GetTokenFromBuffer(string.Empty, false, true, ref buffer, ref token))
                    {
                        lastBufferIndex = 0;
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

                    isWord = false;
                }
                else // any other char - add to buffer
                {
                    isWord = !isOperand; // a word which is not an operand
                    isOperandWord = isOperand; // opperand but not space
                    buffer[lastBufferIndex++] = chr;
                    buffer[lastBufferIndex] = '\0';
                }
            }

            return buffer[lastBufferIndex] == '\0';
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

                buffer[0] = '\0';
            }

            return true;
        }

        #endregion
    }
}