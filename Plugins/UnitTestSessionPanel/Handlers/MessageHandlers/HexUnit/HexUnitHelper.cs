using ASCompletion.Context;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers.HexUnit
{
    public class HexUnitHelper
    {
        private HexUnitMessageConverter converter = new HexUnitMessageConverter();
        private Stack<string> runningSuite;

        public HexUnitHelper()
        {
            runningSuite = new Stack<string>();
        }

        public TestInformation ParseMessage(string msg)
        {
            var hexUnitMessage = JsonConvert.DeserializeObject<HexUnitWebSocketMessage>(msg, converter);

            if (hexUnitMessage.messageType == "testCaseRunSuccess" || hexUnitMessage.messageType == "testCaseRunFailed")
            {
                var data = (HexUnitWebSocketMessageTestData)hexUnitMessage.data;
                TestInformation info = new TestInformation
                {
                    Name = data.methodName,
                    ClassName = data.className,
                    FunctionName = data.methodName,
                    Line = data.lineNumber,
                    RunTime = data.timeElapsed
                };

                if (data.isIgnored)
                {
                    info.Reason = data.description;
                    info.Result = TestResult.Ignored;
                }
                else if (hexUnitMessage.messageType == "testCaseRunSuccess")
                {
                    info.Result = TestResult.Passed;
                }
                else if (hexUnitMessage.messageType == "testCaseRunFailed")
                {
                    info.Result = TestResult.Failed;
                    info.Reason = data.errorMsg;
                }

                var model = ASContext.Context.ResolveType(data.className, null);
                if (model != null) info.Path = model.InFile.FileName;
                if (runningSuite.Count > 0)
                {
                    var suites = runningSuite.ToArray();
                    Array.Reverse(suites);
                    info.OwnerTestSuite = string.Join(".", suites);
                }
                return info;
            }
            else if (hexUnitMessage.messageType == "startRun")  // TODO: Check when we have multiple sessions!
            {
                // Getting start run for each added test to the class
                //pluginUi.ClearStats();
            }
            else if (hexUnitMessage.messageType == "testSuiteStartRun")
            {
                var data = (HexUnitWebSocketMessageTestSuiteData)hexUnitMessage.data;
                runningSuite.Push(data.suiteName);
            }
            else if (hexUnitMessage.messageType == "testSuiteEndRun")
            {
                if (runningSuite.Count > 0) runningSuite.Pop();
            }

            return null;
        }
    }

    public class HexUnitWebSocketMessage
    {
#pragma warning disable 0649
        public string messageId;
        public string clientType;
        public string clientVersion;
        public string clientId;
        public string messageType;
        public object data;
#pragma warning restore 0649
    }

    public class HexUnitWebSocketMessageTestData
    {
#pragma warning disable 0649
        public string className;
        public string methodName;
        public string description;
        public bool isAsync;
        public bool isIgnored;
        public double timeElapsed;
        public string fileName;
        public int lineNumber;
        public string errorMsg;
#pragma warning restore 0649
    }

    public class HexUnitWebSocketMessageTestSuiteData
    {
#pragma warning disable 0649
        public string className;
        public string suiteName;
#pragma warning restore 0649
    }

    public class HexUnitMessageConverter : CustomCreationConverter<HexUnitWebSocketMessage>
    {
        public override HexUnitWebSocketMessage Create(Type objectType)
        {
            throw new NotImplementedException();
        }

        public object Create(Type objectType, JObject jObject)
        {
            var type = (string)jObject.Property("messageType");

            switch (type)
            {
                case "testCaseRunSuccess":
                case "testCaseRunFailed":
                    return new HexUnitWebSocketMessageTestData();
                case "testSuiteStartRun":
                    return new HexUnitWebSocketMessageTestSuiteData();
                default:
                    return null;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            // Populate base message
            var retVal = new HexUnitWebSocketMessage();
            serializer.Populate(jObject.CreateReader(), retVal);

            // Create data object based on type
            var target = Create(objectType, jObject);

            // Populate the object properties 
            if (target != null)
            {
                serializer.Populate(jObject.Property("data").Value.CreateReader(), target);
                retVal.data = target;
            }

            return retVal;
        }
    }
}
