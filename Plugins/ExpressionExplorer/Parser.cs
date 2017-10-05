using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExpressionExplorer
{
    class MyTuple
    {
        public string expression;
        public string position;
        public int parentPosition;
        public int index;

        public MyTuple(string e, string p, int parent, int i)
        {
            expression = e;
            position = p;
            parentPosition = parent;
            index = i;
        }
    }

    class Parser
    {
        public List<MyTuple> result = new List<MyTuple>();

        public Parser()
        {

        }


        private string findFullName(string data, int start, int end, int i)
        {
            string expr = "";
            string toParse = data;

            if (toParse.LastIndexOf(", pos =>") < end)
                expr = toParse.Remove(toParse.LastIndexOf(", pos =>"), end - toParse.LastIndexOf(", pos =>"));
            else
            {
                while (toParse.LastIndexOf(", pos =>") > end)
                    toParse = toParse.Substring(0, toParse.LastIndexOf(", pos =>"));
                expr = toParse;
            }
            expr = expr.Remove(0, start);

            int wpos;

            while ((wpos = expr.IndexOf(", pos =>")) > -1)
                expr = expr.Remove(wpos, expr.IndexOf("}", wpos) - wpos);
            return expr;
        }

        private MyTuple getExpAndPos(int start, int end, string data, int parentPos, int i)
        {
            
            string pos = "";
            string expr = findFullName(data, start, end, i);     
            var tmp = data.ToCharArray();

            for (int j = end - 1; j > -1; j--)
            {
                if (tmp[j] == '}')
                    break;
                else if (tmp[j] == '(')
                {
                    pos = data.Substring(j, end - j - 1);
                    break;
                }
            }
            MessageBox.Show("expr -> " + expr + "\n pos -> " + pos);
            return new MyTuple(expr, pos, parentPos, i);
        }

        public void parseExpressions(string data)
        {
            int parentPos = 0;
            int oB = 0;
            int cB = 0;
            int index = 1;
            var chars = data.ToCharArray();
            List<MyTuple> res = new List<MyTuple>();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '{')
                {
                    for (int j = i + 1; i < chars.Length; j++)
                    {
                        if (chars[j] == '{')
                            oB++;
                        else if (chars[j] == '}')
                        {
                            if (cB == oB)
                            {
                                res.Add(getExpAndPos(i, j, data, parentPos, index));
                                index++;
                                break;
                            }
                            else
                                cB++;
                        }
                    }
                }
                else if (chars[i] == '[')
                    parentPos = index - 1;
                else if (chars[i] == ']')
                    parentPos--;
            }
            result = res;
        }
    }
}
