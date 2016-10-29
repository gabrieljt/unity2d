using System;

public interface IBuildable
{
    void Build();

    Action<Type> Built { get; set; }
}