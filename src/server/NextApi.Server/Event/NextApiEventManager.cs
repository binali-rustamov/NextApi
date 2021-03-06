using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NextApi.Common.Abstractions.Event;
using NextApi.Common.Event;
using NextApi.Server.Request;

namespace NextApi.Server.Event
{
    /// <inheritdoc />
    public class NextApiEventManager : INextApiEventManager
    {
        private readonly INextApiRequest _nextApiRequest;

        /// <inheritdoc />
        public NextApiEventManager(INextApiRequest nextApiRequest)
        {
            _nextApiRequest = nextApiRequest;
        }

        /// <inheritdoc />
        public Task Publish<TEvent, TPayload>(TPayload payload) where TEvent : BaseNextApiEvent<TPayload>
        {
            return InternalPublish<TEvent>(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public Task Publish<TEvent>() where TEvent : BaseNextApiEvent
        {
            return InternalPublish<TEvent>(null);
        }

        private async Task InternalPublish<TEvent>(object payload) where TEvent : INextApiEvent
        {
            if (_nextApiRequest.HubContext == null)
                return;

            await _nextApiRequest.HubContext.Clients.All.SendAsync("NextApiEvent",
                new NextApiEventMessage {EventName = typeof(TEvent).Name, Data = payload});
        }
    }
}
