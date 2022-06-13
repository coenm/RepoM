namespace RepoM.Api.Common;

using System;

public interface IHumanizer
{
    string HumanizeTimestamp(DateTime value);
}