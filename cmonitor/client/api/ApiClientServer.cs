﻿using cmonitor.config;
using common.libs;
using common.libs.api;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace cmonitor.client.api
{

    public interface IApiClientController : IApiController
    {

    }

    public interface IApiClientServer : IApiServer
    {
        public void LoadPlugins(Assembly[] assemblys);
    }



    /// <summary>
    /// 前段接口服务
    /// </summary>
    public sealed class ApiClientServer : ApiServer, IApiClientServer
    {
        private readonly ServiceProvider serviceProvider;
        private readonly Config config;

        public ApiClientServer(ServiceProvider serviceProvider, Config config)
        {
            this.serviceProvider = serviceProvider;
            this.config = config;
        }

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="assemblys"></param>
        public void LoadPlugins(Assembly[] assemblys)
        {
            Type voidType = typeof(void);

            IEnumerable<Type> types = assemblys.SelectMany(c => c.GetTypes()).Where(c => c.GetInterfaces().Contains(typeof(IApiClientController)));

            foreach (Type item in types)
            {
                object obj = serviceProvider.GetService(item);
                if (obj == null)
                {
                    continue;
                }
                Logger.Instance.Warning($"load client api:{item.Name}");

                string path = item.Name.Replace("ApiController", "").Replace("ApiController", "");
                foreach (MethodInfo method in item.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    string key = $"{path}/{method.Name}".ToLower();
                    if (!plugins.ContainsKey(key))
                    {
                        bool istask = method.ReturnType.GetProperty("IsCompleted") != null && method.ReturnType.GetMethod("GetAwaiter") != null;
                        bool isTaskResult = method.ReturnType.GetProperty("Result") != null;
                        plugins.TryAdd(key, new PluginPathCacheInfo
                        {
                            IsVoid = method.ReturnType == voidType,
                            Method = method,
                            Target = obj,
                            IsTask = istask,
                            IsTaskResult = isTaskResult
                        });
                    }
                }
            }
        }
    }
}
