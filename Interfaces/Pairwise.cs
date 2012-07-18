using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace activeWindow
{
    public class Pairwise
    {
        public static ArrayList go(List<PairVaraible> vars, int deep)
        {            
            ArrayList vl = new ArrayList();
            for (int i = 0; i < vars.Count; i ++)
            {
                vl.Add(vars[i].values);
            }

            int m = vl.Count % deep;
            int p = vl.Count / deep;
            ArrayList al = null;
            for (int i = 0; i < p; i++) 
            {
                ArrayList temp = new ArrayList();
                for (int j = 0; j < deep; j++)
                {
                    temp.Add(vl[j + i * deep]);
                }
                if (al == null)
                    al = myPair(temp);
                else
                    al=appendList(al,myPair(temp));
            }

            ArrayList rmain = new ArrayList();
            for (int i = p * deep; i < vl.Count; i++)
            {
                rmain.Add(vl[i]);
            }

            return appendList(al, myPair(rmain));
        }

        private static ArrayList myPair(ArrayList al)
        {
            if (al.Count == 0)
            {
                return null;
            }

            if (al.Count == 1) {
                if (al[0] is ArrayList)
                    return (ArrayList)al[0];
                return al;
            }
            ArrayList arl = new ArrayList();
            int m = al.Count % 2;
            for (int i = 0; i < al.Count-m; i += 2)
            {
                arl.Add(orthog((ArrayList)al[i], (ArrayList)al[i + 1]));
            }
            if (m == 1)
                arl.Add(appendList((ArrayList)al[al.Count - 1], null));

            return myPair(arl);
        }

        private static ArrayList appendList(ArrayList v1, ArrayList v2)
        {
            ArrayList arr = new ArrayList();
            ArrayList max,min;
            if (v2 != null && v2.Count > v1.Count)
            {
                max = v2;
                min = v1;
            }
            else 
            {
                max = v1;
                min = v2;
            }
            for (int i = 0; i < max.Count; i++)
            {
                ArrayList temp;
                if (max[i] is ArrayList)
                {
                    temp = (ArrayList)max[i];
                }
                else
                {
                    temp = new ArrayList();
                    temp.Add(max[i]);
                }
                if (min != null && min.Count > 0)
                {
                    if (min[i % min.Count] is ArrayList)
                    {
                        if (v2.Count > v1.Count)
                            temp.InsertRange(0, (ArrayList)min[i % min.Count]);
                        else 
                            temp.AddRange((ArrayList)min[i % min.Count]);
                    }
                    else
                    {
                        if (v2.Count > v1.Count)
                            temp.Insert(0, min[i % min.Count]);
                        else
                            temp.Add(min[i % min.Count]);
                    }
                }
                arr.Add(temp);
            }
            return arr;

        }
        private static ArrayList orthog(ArrayList v1, ArrayList v2)
        {
            ArrayList arr = new ArrayList();
            for (int i = 0; i < v1.Count; i++)
            {                
                for (int j = 0; j < v2.Count; j++)
                {
                    ArrayList temp = new ArrayList();
                    if (v1[i] is ArrayList)
                        temp.AddRange((ArrayList)v1[i]);
                    else
                        temp.Add(v1[i]);

                    if (v2[j] is ArrayList)
                        temp.AddRange((ArrayList)v2[j]);
                    else
                        temp.Add(v2[j]);

                    arr.Add(temp);
                }
            }
            return arr;
        }

    }

    public class PairVaraible : IComparable<PairVaraible>
    {
        public string name;
        public ArrayList values;
        public int preference;


        #region IComparable<PairVaraible> 成员

        public int CompareTo(PairVaraible other)
        {
            if (values == null || other.values == null)
                return 0;

            return other.values.Count - this.values.Count;
        }

        #endregion
    }
    


}
