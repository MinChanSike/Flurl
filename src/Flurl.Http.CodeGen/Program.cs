﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flurl.Http.CodeGen
{
    class Program
    {
        static int Main(string[] args) {
	        var codePath = (args.Length > 0) ? args[0] : @"..\Flurl.Http.Shared\HttpExtensions.cs";

			if (!File.Exists(codePath)) {
				Console.WriteLine("File not found: " +  Path.GetFullPath(codePath));
				return 2;
			}

			try {
                using (var writer = new CodeWriter(codePath))
                {
                    writer
                        .WriteLine("// This file was auto-generated by Flurl.Http.CodeGen. Do not edit directly.")
                        .WriteLine()
                        .WriteLine("using System.Collections.Generic;")
                        .WriteLine("using System.IO;")
                        .WriteLine("using System.Net.Http;")
                        .WriteLine("using System.Threading;")
                        .WriteLine("using System.Threading.Tasks;")
                        .WriteLine("using Flurl.Http.Content;")
                        .WriteLine("")
                        .WriteLine("namespace Flurl.Http")
                        .WriteLine("{")
                        .WriteLine("    /// <summary>")
                        .WriteLine("/// Http extensions for Flurl Client.")
                        .WriteLine("/// </summary>")
                        .WriteLine("public static class HttpExtensions")
                        .WriteLine("{");

                    WriteExtensionMethods(writer);

                    writer
                        .WriteLine("}")
                        .WriteLine("}");
                }

                Console.WriteLine("File writing succeeded.");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");
                return 2;
            }

            return 0;
        }

        private static void WriteExtensionMethods(CodeWriter writer)
        {
            foreach (var xm in ExtensionMethodModel.GetAll())
            {
                writer.WriteLine("/// <summary>");
                var summaryStart = (xm.ExtentionOfType == "FlurlClient") ? "Sends" : "Creates a FlurlClient from the URL and sends";
                writer.WriteLine("/// @0 an asynchronous @1 request.", summaryStart, xm.HttpVerb.ToUpperInvariant());
                writer.WriteLine("/// </summary>");
                if (xm.BodyType != null)
                    writer.WriteLine("/// <param name=\"data\">Contents of the request body.</param>");
                if (xm.ExtentionOfType == "FlurlClient")
                    writer.WriteLine("/// <param name=\"client\">The Flurl client.</param>");
                if (xm.ExtentionOfType == "Url")
                    writer.WriteLine("/// <param name=\"url\">The URL.</param>");
                if (xm.ExtentionOfType == "string")
                    writer.WriteLine("/// <param name=\"url\">The URL.</param>");
                if (xm.HasCancelationToken)
                    writer.WriteLine("/// <param name=\"cancellationToken\">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>");
                writer.WriteLine("/// <returns>A Task whose result is @0.</returns>", xm.ReturnTypeDescription);

                var args = new List<string>();
                args.Add("this " + xm.ExtentionOfType + (xm.ExtentionOfType == "FlurlClient" ? " client" : " url"));
                if (xm.BodyType != null)
                    args.Add((xm.BodyType == "String" ? "string" : "object") + " data");
                if (xm.HasCancelationToken)
                    args.Add("CancellationToken cancellationToken");

                writer.WriteLine("public static Task<@0> @1@2(@3) {", xm.TaskArg, xm.Name, xm.IsGeneric ? "<T>" : "", string.Join(", ", args));

                if (xm.ExtentionOfType == "FlurlClient")
                {
                    if (xm.BodyType != null)
                    {
                        writer.WriteLine("var content = new Captured@0Content(@1);",
                            xm.BodyType,
                            xm.BodyType == "String" ? "data" : string.Format("client.Settings.{0}Serializer.Serialize(data)", xm.BodyType));
                    }

                    args.Clear();
                    args.Add(xm.HttpVerb == "Patch" ? "new HttpMethod(\"PATCH\")" : "HttpMethod." + xm.HttpVerb); // there's no HttpMethod.Patch
                    if (xm.BodyType != null)
                        args.Add("content: content");
                    if (xm.HasCancelationToken)
                        args.Add("cancellationToken: cancellationToken");

                    var client = (xm.ExtentionOfType == "FlurlClient") ? "client" : "new FlurlClient(url, false)";
                    var receive = (xm.DeserializeToType == null) ? "" : string.Format(".Receive{0}{1}()", xm.DeserializeToType, xm.IsGeneric ? "<T>" : "");
                    writer.WriteLine("return @0.SendAsync(@1)@2;", client, string.Join(", ", args), receive);
                }
                else
                {
                    writer.WriteLine("return new FlurlClient(url, false).@0(@1);",
                        xm.Name + (xm.IsGeneric ? "<T>" : ""),
                        string.Join(", ", args.Skip(1).Select(a => a.Split(' ').Last())));
                }

                writer.WriteLine("}").WriteLine();
            }
        }
    }
}