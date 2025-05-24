using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlaywrightLang.LanguageServices.Object.Primitive;

internal class PwList : PwObjectClass
{
    public List<PwInstance> innerList;
    
    [PwItem("length")]
    public float Length => innerList.Count;
    
    public PwList(IEnumerable inner)
    {
        innerList = new List<PwInstance>();
        foreach (var VARIABLE in inner)
        {
            innerList.Add(VARIABLE.AsPwInstance());
        }
    }

    [PwItem("__indg__")]
    public object PwIndex(float index)
    {
        return innerList[(int)Math.Floor(index)];
    }

    [PwItem("__inds__")]
    public void PwIndexSet(float index, object item)
    {
        innerList[(int)Math.Floor(index)] = item.AsPwInstance();
    }

    [PwItem("append")]
    public void Append(object value)
    {
        innerList.Add(value.AsPwInstance());
    }

    [PwItem("remove_at")]
    public void Remove(float i)
    {
        innerList.RemoveAt((int)Math.Floor(i));
    }
}