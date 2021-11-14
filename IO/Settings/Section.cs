using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Paulus.Collections;

namespace Paulus.IO.Settings
{
    public class Section : IEnumerable<string>, IEnumerable<KeyValuePair<string, string>>
    {
        public Section() { }

        public Section(string name, int positionIndex) { Name = name; PositionIndex = positionIndex; }

        //public Section(string name, int index, IEnumerable<string> data) :this(name,0)
        //{
        //    Lines.AddRange(data); 
        //}

        public string Name { get; set; }

        /// <summary>
        /// The index position of the section inside the file starting from 0.
        /// </summary>
        public int PositionIndex { get; set; }

        private bool _selected = true;
        /// <summary>
        /// An unselected section is not written to a file when the Write method is called from its parent object. Default is true.
        /// </summary>
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        /// <summary>
        /// Returns true if the local dictionary is populated with entries. Entries may be populated if there are lines with "=" inside them.
        /// </summary>
        public bool IsDictionaryPopulated { get { return Dictionary.Count > 0; } }

        public List<string> Lines = new List<string>();

        ////the definitions are populated when the dictionary is populated
        //public List<KeyValuePair<string, string>> Definitions = new List<KeyValuePair<string, string>>();

        public OrderedDictionary<string, string> Dictionary = new OrderedDictionary<string, string>();
        /// <summary>
        /// Returns a definition from the section. Each definition must be defined once.
        /// </summary>
        /// <param name="key">The key of the entry in the section's dictionary.</param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return Dictionary[key];
            }
            set { Dictionary[key] = value; }
        }

        /// <summary>
        /// Uses when the dictionary is populated. Returns the value if it exists. If it does not or it cannot be parsed, it returns the default value of the type.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The name of the entry at the local dictionary.</param>
        /// <returns>The entry value if it exists or the default value of the type if it does not.</returns>
        public T GetValueOrNull<T>(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                try
                {
                    Type type =typeof(T);
                     if(   Nullable.GetUnderlyingType(typeof(T)) != null)
                         type=Nullable.GetUnderlyingType(typeof(T));

                    T result = (T)Convert.ChangeType(Dictionary[key].ToLower(), type);
                    return result;
                }
                catch
                {
                    return default(T);
                }
            }
            else return default(T);
        }

        /// <summary>
        /// Returns a specific line from a section.
        /// </summary>
        /// <param name="index">The number of the line to be returned beginning from 0.</param>
        /// <returns></returns>
        public string this[int index]
        {
            get
            {
                //if (IsDictionaryPopulated)
                //    return Dictionary[index];
                //else
                return Lines[index];
            }
        }

        public int LineCount { get { return Lines.Count; } }

        public int DictionaryEntriesCount { get { return Dictionary.Count; } }

        public void PopulateDictionary(bool keyIsLeft = true, bool ignoreDuplicateKeys = true)
        {
            foreach (string record in Lines)
            {
                //add only those records that have an '=' and there is at least one character before it
                int equalPosition = record.IndexOf('=');
                if (equalPosition > 0)
                {
                    string left = record.Substring(0, equalPosition);
                    string right = record.Substring(equalPosition + 1);
                    string key = keyIsLeft ? left : right;
                    string value = keyIsLeft ? right : left;
                    if (Dictionary.ContainsKey(key) && ignoreDuplicateKeys) continue;

                    try { Dictionary.Add(key, value); }
                    catch (ArgumentException) { throw; } //duplicates rethrow from here!
                }
            }
        }

        #region Enumerators
        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        #endregion
        #endregion

        public override string ToString()
        {
            return this.Name;
        }
    }
}
