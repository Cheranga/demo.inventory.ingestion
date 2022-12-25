﻿using LanguageExt.Common;

namespace Infrastructure.Messaging.Azure.Queues.Demo;

public class MessagePublishException : Exception
{
    public MessagePublishException(Error error)
    {
        Error = error;
    }

    public Error Error { get; }
}