using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterLog_Backend
{
    //Class That Holds Datapoints neccessary for graphing
    public class DataPoints<T, Y>
    {
        public List<DataNode<T,Y>> dataPoints; 

        public DataPoints()
        {
            dataPoints = new List<DataNode<T, Y>>();
        }

        public void AddPoint(T xVal, Y yVal)
        {
            DataNode<T, Y> node = new DataNode<T, Y>();
            node.x = xVal;
            node.y = yVal;
            dataPoints.Add(node);

        }
        
        public bool Equals(DataPoints<T,Y> obj)
        {
            if (obj.dataPoints.Count != this.dataPoints.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < this.dataPoints.Count; i++)
                {
                    if (!obj.dataPoints.ElementAt(i).Equals(this.dataPoints.ElementAt(i)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
         public List<Y> getValueY()
        {
            List<Y> outV = new List<Y>() ; 
            foreach (DataNode<T, Y> listV in dataPoints)
            {
                outV.Add(listV.y);     
            }
            return outV;
        }

        public List<T> getvalueT()
        {
            List<T> outV = new List<T>();
            foreach (DataNode<T, Y> listV in dataPoints)
            {
                outV.Add(listV.x);
            }
            return outV;
        }
    }

    //Unit of Data that will be plotted
    public struct DataNode<T, Y>
    {
        public T x;
        public Y y;
    }
}
