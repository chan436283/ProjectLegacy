using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWFramework
{
    [System.Serializable]
    public class Deck<T>
    {
        public List<T> elements = new();

        public bool AllowDuplicateValues;

        public int Count => elements.Count;

        public Deck()
        {
        }

        public Deck(T[] values)
        {
            elements.AddRange(values);
        }

        public Deck(IList<T> values)
        {
            elements.AddRange(values);
        }

        public bool Contains(T content)
        {
            return elements.Contains(content);
        }

        public void Add(T content)
        {
            if (AllowDuplicateValues)
            {
                elements.Add(content);
            }
            else if (false == elements.Contains(content))
            {
                elements.Add(content);
            }
        }

        public void AddRange(T[] contents)
        {
            if (contents == null)
            {
                return;
            }

            foreach (T content in contents)
            {
                Add(content);
            }
        }

        public void AddRange(IList<T> contents)
        {
            if (contents == null)
            {
                return;
            }

            foreach (T content in contents)
            {
                Add(content);
            }
        }

        public void Remove(T content)
        {
            if (elements.Contains(content))
            {
                elements.Remove(content);
            }
            else
            {
                Log.Error("Deck::Remove() 콘텐츠를 제거하지 못했습니다. 덱에 콘텐츠가 포함되어 있지 않습니다. {0}", content.ToString());
            }
        }

        public void RemoveAt(int index)
        {
            if (elements.IsValid(index))
            {
                elements.RemoveAt(index);
            }
            else
            {
                Log.Error("Deck::Remove() 콘텐츠를 제거하지 못했습니다. 덱의 개수가 인덱스보다 적습니다. index: {0}, count: {1}", index.ToString(), Count.ToString());
            }
        }

        public void Set(int index, T content)
        {
            if (elements.IsValid(index))
            {
                elements[index] = content;
            }
        }

        public void Set(T[] contents)
        {
            if (contents == null || contents.Length == 0)
            {
                return;
            }

            Clear();
            elements.AddRange(contents);
        }

        public void Set(IList<T> contents)
        {
            if (contents == null || contents.Count == 0)
            {
                return;
            }

            Clear();
            elements.AddRange(contents);
        }

        public void Clear()
        {
            if (elements.Count > 0)
            {
                elements.Clear();
            }
        }

        public T Get(int index)
        {
            if (elements.IsValid(index))
            {
                return elements[index];
            }

            return default;
        }

        public T DrawTop() // 이전: Pick
        {
            if (elements.Count != 0)
            {
                T pickedValue = elements[0];
                elements.RemoveAt(0);
                return pickedValue;
            }
            else
            {
                Log.Error("Deck::DrawTop() 선택에 실패했습니다. 덱 수가 0입니다.");
            }

            return default;
        }

        public void Shuffle()
        {
            if (elements.Count > 1)
            {
                InfiniteLoopDetector.Reset();

                int number = elements.Count;
                while (number > 1)
                {
                    number--;

                    int temp = CWRandomEx.Range(0, number + 1);

                    (elements[number], elements[temp]) = (elements[temp], elements[number]);
                    InfiniteLoopDetector.Run();
                }
            }
        }

        public T[] ToArray()
        {
            return elements.ToArray();
        }

        public List<T> ToList()
        {
            return elements.ToList();
        }

        public string JoinToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in elements)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append(',');
            }

            return stringBuilder.ToString();
        }
    }
}