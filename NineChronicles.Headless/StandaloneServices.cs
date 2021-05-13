using System;
using Libplanet.Net;
using Libplanet.Headless;
using NineChronicles.Headless.Properties;

namespace NineChronicles.Headless
{
    public static class StandaloneServices
    {
        public static NineChroniclesNodeService CreateHeadless(
            NineChroniclesNodeServiceProperties properties,
            StandaloneContext? standaloneContext = null,
            bool ignoreBootstrapFailure = true,
            bool ignorePreloadFailure = true,
            bool strictRendering = false,
            bool authorizedMiner = false,
            bool isDev = false,
            int blockInterval = 10000,
            int reorgInterval = 0,
            TimeSpan txLifeTime = default
        )
        {
            if (standaloneContext is null)
            {
                throw new InvalidOperationException($"{nameof(standaloneContext)} is null.");
            }
            
            Progress<PreloadState> progress = new Progress<PreloadState>(state =>
                {
                    standaloneContext.PreloadStateSubject.OnNext(state);
                });
                
            if (properties.Libplanet is null)
            {
                throw new InvalidOperationException($"{nameof(properties.Libplanet)} is null.");
            }

            properties.Libplanet.DifferentAppProtocolVersionEncountered =
                (Peer peer, AppProtocolVersion peerVersion, AppProtocolVersion localVersion) =>
                {
                    standaloneContext.DifferentAppProtocolVersionEncounterSubject.OnNext(
                        new DifferentAppProtocolVersionEncounter(peer, peerVersion, localVersion)
                    );

                    // FIXME: 일단은 버전이 다른 피어는 마주쳐도 쌩깐다.
                    return false;
                };

            properties.Libplanet.NodeExceptionOccurred =
                (code, message) =>
                {
                    standaloneContext.NodeExceptionSubject.OnNext(
                        new NodeException(code, message)
                    );
                };

            var service = new NineChroniclesNodeService(
                properties.MinerPrivateKey,
                properties.Libplanet,
                properties.Rpc,
                preloadProgress: progress,
                ignoreBootstrapFailure: ignoreBootstrapFailure,
                ignorePreloadFailure: ignorePreloadFailure,
                strictRendering: strictRendering,
                isDev: isDev,
                blockInterval: blockInterval,
                reorgInterval: reorgInterval,
                authorizedMiner: authorizedMiner,
                txLifeTime: txLifeTime);
            service.ConfigureContext(standaloneContext);
            return service;
        }
    }
}
