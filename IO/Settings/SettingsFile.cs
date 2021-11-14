using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Paulus.Common;

using Paulus.Collections;

//TODO: comments are not saved or stored for the moment
//they are ignored at the reading and not written at the exit!

namespace Paulus.IO.Settings
{
    //to allow saving to a file, an object could be bound to a line
    public class SettingsFile : IDisposable, IEnumerable<KeyValuePair<string, Section>>, IEnumerable<Section>
    {
        #region Constructors
        public SettingsFile()
        {
            LoadDefaultFileSettings();
        }

        public SettingsFile(string path, bool readNow = false)
            : this()
        {
            _path = path;
            if (readNow) Read();
        }

        public SettingsFile(StreamReader reader)
            : this()
        {
            Read(reader);
        }

        public void LoadDefaultFileSettings()
        {
            _trimDataLines = true; //trims trailing whitespace from data lines
            _allowComments = true; //all characters after the comment character are trimmed (including the comment character)
            _commentCharacter = ';';
            _allowEmptyLines = true; //allow empty lines inside sections
            _sections = new OrderedDictionary<string, Section>();
        }

        public static SettingsFile GetSettingsFileFromFirstArgument(bool populateDictionaries = true)
        {
            //the arguments are NOT inside quotation marks and the args are the name of the parameter
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1) return null;

            string configPath = args[1];

            SettingsFile settings = new SettingsFile(configPath, true);
            if (populateDictionaries) settings.PopulateDictionary();

            return settings;
        }
        #endregion

        #region Main Properties
        protected bool _trimDataLines; //trims trailing whitespace from data lines
        public bool TrimDataLines { get { return _trimDataLines; } set { _trimDataLines = value; } }

        protected bool _allowComments; //all characters after the comment character are not taken into account
        public bool AllowComments { get { return _allowComments; } set { _allowComments = value; } }

        protected char _commentCharacter; //set the comment character
        public char CommentCharacter { get { return _commentCharacter; } set { _commentCharacter = value; } }

        protected bool _allowEmptyLines; //allows empty lines inside sections
        public bool AllowEmptyLines { get { return _allowEmptyLines; } set { _allowEmptyLines = value; } }

        protected string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                //do not throw an exception here because the file may be created (in theory) later
                //so only during the read operation the error should be thrown
                if (_path == value) return;
            }
        }
        #endregion

        #region Sections
        //raised when started reading a section
        public event EventHandler<EventArgs<Section>> SectionReading;
        protected void OnSectionReading(Section s)
        {
            var handler = SectionReading;
            if (handler != null) handler(this, new EventArgs<Section>(s));
        }

        //raised when finished reading a section
        public event EventHandler<EventArgs<Section>> SectionRead;
        protected void OnSectionRead(Section s)
        {
            var handler = SectionRead;
            if (handler != null) handler(this, new EventArgs<Section>(s));
        }

        private OrderedDictionary<string, Section> _sections;
        public OrderedDictionary<string, Section> Sections { get { return _sections; } }

        /// <summary>
        /// Returns a section from it's corresponding name. All sections are stored in the Sections Dictionary internal object.
        /// </summary>
        /// <param name="name">The name of the section to return.</param>
        /// <returns></returns>
        public Section this[string name]
        {
            get
            {
                return _sections[name];
            }//read-only accessor

        }

        /// <summary>
        /// Returns a section by it's index.
        /// </summary>
        /// <param name="index">The index of the section to return starting from 0.</param>
        /// <returns></returns>
        public Section this[int index]
        {
            get
            {
                //return _sections.ElementAt<KeyValuePair<string,Section>>(index).Value;
                return _sections[index];
            }
        }

        public void PopulateDictionary(bool keyIsLeft = true)
        {
            foreach (KeyValuePair<string, Section> entry in _sections)
                entry.Value.PopulateDictionary(keyIsLeft);
        }


        #endregion

        #region Read

        /// <summary>
        /// Reads the file in the path property. All sections are loaded in the Sections property. A user can hook a handler to read only a specified section -avoiding thus to read the whole file.
        /// </summary>
        public void Read()
        {
            Read(_path, Encoding.Default);
        }

        public void Read(string filePath, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(filePath, encoding))
                Read(reader);
        }

        /// <summary>
        /// Reads the filePath file. All sections are loaded in the Sections property. A user can hook a handler to read only a specified section -avoiding thus to read the whole file.
        /// No error handling occurs here, so the user should call this method in a try,catch structure.
        /// </summary>
        public void Read(StreamReader reader)
        {
            int iCurrentSection = 0;
            Section currentSection = null; //we don't have any current section (but we may have one at the end of the file)

            _sections.Clear(); //clear previously loaded sections
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                //remove comments if those are allowed
                bool hasComment = false;
                if (_allowComments)
                {
                    int commentCharPosition = line.IndexOf(_commentCharacter);
                    hasComment = commentCharPosition >= 0;
                    if (hasComment) line = line.Substring(0, commentCharPosition);
                }

                string trimmedLine = line.Trim();
                int trimmedLineLength = trimmedLine.Length;
                //an empty line that has a comment only
                bool isCommentLine = _allowComments && hasComment && trimmedLine.Length == 0;

                //check for a new section
                bool isNewSection = trimmedLineLength > 0 && trimmedLine[0] == '[' && trimmedLine[trimmedLineLength - 1] == ']';

                //check if a previous section needs to be closed (2 cases)
                bool closePreviousSection = currentSection != null &&
                    (isNewSection ||
                    !isCommentLine && (!_allowEmptyLines && trimmedLineLength == 0));
                if (closePreviousSection)
                {
                    //close previous section and add to the collection
                    _sections.Add(currentSection.Name, currentSection);
                    //raise event
                    OnSectionRead(currentSection);
                    //check if a new section begins or not
                    if (!isNewSection) currentSection = null;
                    else
                    {
                        string name = trimmedLine.Substring(1, trimmedLineLength - 2);
                        currentSection = new Section(name, iCurrentSection++);
                        OnSectionReading(currentSection);
                    }
                }
                //a new section WITHOUT closing a previous one may occur at the first section
                else if (isNewSection)
                {
                    string name = trimmedLine.Substring(1, trimmedLineLength - 2);
                    currentSection = new Section(name, iCurrentSection++);
                    OnSectionReading(currentSection);
                }

              //add data (do not add if empty lines are met)
                else if (currentSection != null && trimmedLineLength > 0)
                    currentSection.Lines.Add(_trimDataLines ? trimmedLine : line);
            }


            //at the end add the current section to the collection
            if (currentSection != null)
            {
                //close previous section and add to the collection
                _sections.Add(currentSection.Name, currentSection);
                //raise event
                OnSectionRead(currentSection);
            }
        }

        public void Save(StreamWriter writer, int numberOfEmptyLinesBetweenSections = 1, bool keyIsLeft = true)
        {
            foreach (KeyValuePair<string, Section> entry in _sections)
            {
                Section section = entry.Value;
                if (section.Selected) //section should be selected or it is not written to the file
                {
                    writer.WriteLine("[" + section.Name + "]");
                    if (section.Dictionary.Count > 0)
                        if (keyIsLeft)
                            foreach (var dictionaryEntry in section.Dictionary)
                                writer.WriteLine(dictionaryEntry.Key + "=" + dictionaryEntry.Value);
                        else
                            foreach (var dictionaryEntry in section.Dictionary)
                                writer.WriteLine(dictionaryEntry.Value + "=" + dictionaryEntry.Key);
                    else
                        foreach (string data in section.Lines)
                            writer.WriteLine(data);

                    for (int iLine = 0; iLine < numberOfEmptyLinesBetweenSections; iLine++) writer.WriteLine();
                }
            }
        }

        public void Save(string filePath, bool append, Encoding encoding, int numberOfEmptyLinesBetweenSections = 1, bool keyIsLeft = true)
        {
            using (StreamWriter writer = new StreamWriter(filePath, append, encoding))
                Save(writer, numberOfEmptyLinesBetweenSections, keyIsLeft);
        }

        //System.Text.Encoding.ASCII
        public void Save() { Save(_path, false, Encoding.Default); }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            SectionRead.RemoveHandlers();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Section>> Members

        public IEnumerator<KeyValuePair<string, Section>> GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        #endregion

        IEnumerator<Section> IEnumerable<Section>.GetEnumerator()
        {
            foreach (var entry in _sections)
                yield return entry.Value;
        }
    }

}
