/**
* Copyright 2018, 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using System.Collections.Generic;
using System.Text;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.Assistant.V2.Model;
using Newtonsoft.Json;
using System;
using UnityEngine.Networking;

namespace IBM.Watson.Assistant.V2
{
    public class AssistantService : BaseService
    {
        private const string serviceId = "conversation";
        private const string defaultUrl = "https://gateway.watsonplatform.net/assistant/api";

        #region Credentials
        /// <summary>
        /// Gets and sets the credentials of the service. Replace the default endpoint if endpoint is defined.
        /// </summary>
        public Credentials Credentials
        {
            get { return credentials; }
            set
            {
                credentials = value;
                if (!string.IsNullOrEmpty(credentials.Url))
                {
                    Url = credentials.Url;
                }
            }
        }
        #endregion

        #region Url
        /// <summary>
        /// Gets and sets the endpoint URL for the service.
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        #endregion

        #region VersionDate
        private string versionDate;
        /// <summary>
        /// Gets and sets the versionDate of the service.
        /// </summary>
        public string VersionDate
        {
            get { return versionDate; }
            set { versionDate = value; }
        }
        #endregion

        #region DisableSslVerification
        private bool disableSslVerification = false;
        /// <summary>
        /// Gets and sets the option to disable ssl verification
        /// </summary>
        public bool DisableSslVerification
        {
            get { return disableSslVerification; }
            set { disableSslVerification = value; }
        }
        #endregion

        /// <summary>
        /// AssistantService constructor.
        /// </summary>
        /// <param name="versionDate">The service version date in `yyyy-mm-dd` format.</param>
        public AssistantService(string versionDate) : base(versionDate, serviceId)
        {
            VersionDate = versionDate;
        }

        /// <summary>
        /// AssistantService constructor.
        /// </summary>
        /// <param name="versionDate">The service version date in `yyyy-mm-dd` format.</param>
        /// <param name="credentials">The service credentials.</param>
        public AssistantService(string versionDate, Credentials credentials) : base(versionDate, credentials, serviceId)
        {
            if (string.IsNullOrEmpty(versionDate))
            {
                throw new ArgumentNullException("A versionDate (format `yyyy-mm-dd`) is required to create an instance of AssistantService");
            }
            else
            {
                VersionDate = versionDate;
            }

            if (credentials.HasCredentials() || credentials.HasIamTokenData())
            {
                Credentials = credentials;

                if (string.IsNullOrEmpty(credentials.Url))
                {
                    credentials.Url = defaultUrl;
                }
            }
            else
            {
                throw new IBMException("Please provide a username and password or authorization token to use the Assistant service. For more information, see https://github.com/watson-developer-cloud/unity-sdk/#configuring-your-service-credentials");
            }
        }

        /// <summary>
        /// Create a session.
        ///
        /// Create a new session. A session is used to send user input to a skill and receive responses. It also
        /// maintains the state of the conversation.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="assistantId">Unique identifier of the assistant. You can find the assistant ID of an assistant
        /// on the **Assistants** tab of the Watson Assistant tool. For information about creating assistants, see the
        /// [documentation](https://console.bluemix.net/docs/services/assistant/create-assistant.html#creating-assistants).
        ///
        /// **Note:** Currently, the v2 API does not support creating assistants.</param>
        /// <returns><see cref="SessionResponse" />SessionResponse</returns>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        public bool CreateSession(Callback<SessionResponse> callback, string assistantId, Dictionary<string, object> customData = null)
        {
            if (callback == null)
                throw new ArgumentNullException("A callback is required for CreateSession");
            if (string.IsNullOrEmpty(assistantId))
                throw new ArgumentNullException("assistantId is required for CreateSession");

            RequestObject<SessionResponse> req = new RequestObject<SessionResponse>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            req.Headers["X-IBMCloud-SDK-Analytics"] = "service_name=conversation;service_version=V2;operation_id=CreateSession";
            req.Parameters["version"] = VersionDate;

            req.OnResponse = OnCreateSessionResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v2/assistants/{0}/sessions", assistantId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnCreateSessionResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<SessionResponse> response = new DetailedResponse<SessionResponse>();
            Dictionary<string, object> customData = ((RequestObject<SessionResponse>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<SessionResponse>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("AssistantService.OnCreateSessionResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<SessionResponse>)req).Callback != null)
                ((RequestObject<SessionResponse>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Delete session.
        ///
        /// Deletes a session explicitly before it times out.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="assistantId">Unique identifier of the assistant. You can find the assistant ID of an assistant
        /// on the **Assistants** tab of the Watson Assistant tool. For information about creating assistants, see the
        /// [documentation](https://console.bluemix.net/docs/services/assistant/create-assistant.html#creating-assistants).
        ///
        /// **Note:** Currently, the v2 API does not support creating assistants.</param>
        /// <param name="sessionId">Unique identifier of the session.</param>
        /// <returns><see cref="object" />object</returns>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        public bool DeleteSession(Callback<object> callback, string assistantId, string sessionId, Dictionary<string, object> customData = null)
        {
            if (callback == null)
                throw new ArgumentNullException("A callback is required for DeleteSession");
            if (string.IsNullOrEmpty(assistantId))
                throw new ArgumentNullException("assistantId is required for DeleteSession");
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException("sessionId is required for DeleteSession");

            RequestObject<object> req = new RequestObject<object>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbDELETE,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            req.Headers["X-IBMCloud-SDK-Analytics"] = "service_name=conversation;service_version=V2;operation_id=DeleteSession";
            req.Parameters["version"] = VersionDate;

            req.OnResponse = OnDeleteSessionResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v2/assistants/{0}/sessions/{1}", assistantId, sessionId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnDeleteSessionResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<object> response = new DetailedResponse<object>();
            Dictionary<string, object> customData = ((RequestObject<object>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<object>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("AssistantService.OnDeleteSessionResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<object>)req).Callback != null)
                ((RequestObject<object>)req).Callback(response, resp.Error, customData);
        }
        /// <summary>
        /// Send user input to assistant.
        ///
        /// Send user input to an assistant and receive a response.
        ///
        /// There is no rate limit for this operation.
        /// </summary>
        /// <param name="callback">The callback function that is invoked when the operation completes.</param>
        /// <param name="assistantId">Unique identifier of the assistant. You can find the assistant ID of an assistant
        /// on the **Assistants** tab of the Watson Assistant tool. For information about creating assistants, see the
        /// [documentation](https://console.bluemix.net/docs/services/assistant/create-assistant.html#creating-assistants).
        ///
        /// **Note:** Currently, the v2 API does not support creating assistants.</param>
        /// <param name="sessionId">Unique identifier of the session.</param>
        /// <param name="request">The message to be sent. This includes the user's input, along with optional content
        /// such as intents and entities. (optional)</param>
        /// <returns><see cref="MessageResponse" />MessageResponse</returns>
        /// <param name="customData">A Dictionary<string, object> of data that will be passed to the callback. The raw
        /// json output from the REST call will be passed in this object as the value of the 'json'
        /// key.</string></param>
        public bool Message(Callback<MessageResponse> callback, string assistantId, string sessionId, MessageRequest request = null, Dictionary<string, object> customData = null)
        {
            if (callback == null)
                throw new ArgumentNullException("A callback is required for Message");
            if (string.IsNullOrEmpty(assistantId))
                throw new ArgumentNullException("assistantId is required for Message");
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException("sessionId is required for Message");

            RequestObject<MessageResponse> req = new RequestObject<MessageResponse>
            {
                Callback = callback,
                HttpMethod = UnityWebRequest.kHttpVerbPOST,
                DisableSslVerification = DisableSslVerification,
                CustomData = customData == null ? new Dictionary<string, object>() : customData
            };

            if (req.CustomData.ContainsKey(Constants.String.CUSTOM_REQUEST_HEADERS))
            {
                foreach (KeyValuePair<string, string> kvp in req.CustomData[Constants.String.CUSTOM_REQUEST_HEADERS] as Dictionary<string, string>)
                {
                    req.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            req.Headers["X-IBMCloud-SDK-Analytics"] = "service_name=conversation;service_version=V2;operation_id=Message";
            req.Parameters["version"] = VersionDate;
            req.Headers["Content-Type"] = "application/json";
            req.Headers["Accept"] = "application/json";
            if (request != null)
            {
                req.Send = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            }

            req.OnResponse = OnMessageResponse;

            RESTConnector connector = RESTConnector.GetConnector(Credentials, string.Format("/v2/assistants/{0}/sessions/{1}/message", assistantId, sessionId));
            if (connector == null)
            {
                return false;
            }

            return connector.Send(req);
        }

        private void OnMessageResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<MessageResponse> response = new DetailedResponse<MessageResponse>();
            Dictionary<string, object> customData = ((RequestObject<MessageResponse>)req).CustomData;
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<MessageResponse>(json);
                customData.Add("json", json);
            }
            catch (Exception e)
            {
                Log.Error("AssistantService.OnMessageResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RequestObject<MessageResponse>)req).Callback != null)
                ((RequestObject<MessageResponse>)req).Callback(response, resp.Error, customData);
        }
    }
}