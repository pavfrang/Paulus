using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using DevExpress.XtraBars.Ribbon;
using Paulus.IO;
using Paulus.Collections;

namespace Paulus.UI
{
    public class RecentFiles
    {
        public RecentFiles(RecentStackPanel stackPanel, string path)
        {
            StackPanel = stackPanel;

            LoadRecentsFile(path);
        }

        RecentStackPanel StackPanel { get; }

        BiDictionary<RecentPinItem, RecentFileItem> Items;

        public void LoadRecentsFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("File does not exist.", path);

            //<?xml version="1.0" encoding="utf-8" ?>
            //<files>
            //  <file date="2017/09/12 11:30:00" path="d:\config1.path" pinned="yes" />
            //  <file date="2017/09/08 11:30:00" path="d:\config2.path" pinned="no" />
            //  <file date="2017/03/12 11:30:00" path="d:\config3.path" pinned="no" />
            //  <file date="2017/09/12 11:30:00" path="d:\config4.path" pinned="no" />
            //  <file date="2017/09/12 11:30:00" path="d:\config5.path" pinned="no" />
            //</files>

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                var nodeList = doc.DocumentElement.SelectNodes("file").Cast<XmlElement>();

                Items = new BiDictionary<RecentPinItem, RecentFileItem>();
                foreach (XmlElement element in nodeList)
                {
                    RecentFileItem newRecentItem = new RecentFileItem(element);

                    //ignore then non-existing files
                    if (!File.Exists(newRecentItem.FilePath)) continue;

                    RecentPinItem item = newRecentItem.ToRecentPinItem();
                    item.PinButtonCheckedChanged += (o, e) =>
                    {
                        //update internal item
                        Items[item].Pinned = item.PinButtonChecked;
                        //repopulate the list
                        populateRecentItems();
                    };
                    Items.Add(item, newRecentItem);
                }

                populateRecentItems();
            }
            catch { }
            //file is not valid recent file do not crash here

        }

        private void populateRecentItems()
        {
            // StackPanel.MovePinnedItemsUp = true;
            StackPanel.Items.Clear();

            var pinnedItems = Items.Where(e => e.Value.Pinned).ToList();
            if (pinnedItems.Any())
            {
                StackPanel.Items.Add(new RecentLabelItem() { Caption = "Pinned", Style = RecentLabelStyles.Large });
                foreach (var entry in pinnedItems)
                    StackPanel.Items.Add(entry.Key);
            }

            if (pinnedItems.Any())
                StackPanel.Items.Add(new RecentSeparatorItem());

            //should check the last day accessed (Yesterday, Last Week, Last Month, Older)

            //check for the yesterday files 

            var nonPinnedItems = Items.Where(e => !e.Value.Pinned);

            DateTime now = DateTime.Now;

            var alreadyIncludedPins = addPinsBasedOnDate("Today", (d) =>
                (now - d).TotalDays < 1.0 && now.Day == d.Day, nonPinnedItems);

            alreadyIncludedPins.AddRange(
                addPinsBasedOnDate("Yesterday", (d) =>
               (now - d).TotalDays < 2.0 && (now.Day - d.Day) == 1, nonPinnedItems.Except(alreadyIncludedPins)));

            alreadyIncludedPins.AddRange(
                addPinsBasedOnDate("This Week", (d) =>
                (now - d).TotalDays < 8.0 && (now.Day - d.Day) <= 7, nonPinnedItems.Except(alreadyIncludedPins)));

            alreadyIncludedPins.AddRange(
            addPinsBasedOnDate("Last Week", (d) =>
                (now - d).TotalDays < 8.0 && (now.Day - d.Day) <= 7, nonPinnedItems.Except(alreadyIncludedPins))); ;

            addPinsBasedOnDate("Older", (d) => true, nonPinnedItems.Except(alreadyIncludedPins));
        }

        protected List<KeyValuePair<RecentPinItem, RecentFileItem>> addPinsBasedOnDate(string label,
            Func<DateTime, bool> dateFilter,
            IEnumerable<KeyValuePair<RecentPinItem, RecentFileItem>> items)
        {
            //var nonPinnedItems = items.Where(e => !e.Value.Pinned);
            var pins = items.Where(e => dateFilter(e.Value.LastAccessedDate));
            if (pins.Any())
            {
                StackPanel.Items.Add(new RecentLabelItem() { Caption = label, Style = RecentLabelStyles.Large });
                foreach (var entry in pins)
                    StackPanel.Items.Add(entry.Key);
            }
            return pins.ToList();
        }

        public void AddOrUpdate(string fileName)
        {
            //check if the file is already added
            var foundRecentFile = Items.Values.FirstOrDefault(r =>
                Path.GetFullPath(r.FilePath).Equals(Path.GetFullPath(fileName), StringComparison.OrdinalIgnoreCase));

            if (foundRecentFile == null)
            {
                //make a new pin
                RecentFileItem r = new RecentFileItem(fileName, false, DateTime.Now);
                RecentPinItem p = r.ToRecentPinItem();
                p.PinButtonCheckedChanged += (o, e) =>
                {
                    //update internal item
                    Items[p].Pinned = p.PinButtonChecked;
                    //repopulate the list
                    populateRecentItems();
                };
                Items.Add(p, r);
            }
            else // a new pin is not needed
                foundRecentFile.LastAccessedDate = DateTime.Now;
            populateRecentItems();
        }


        public void SaveAs(string path)
        {
            using (StreamWriter writer = XmlExtensions.GetWriterAndWriteProlog(path))
            {
                writer.WriteLine("<files>");
                foreach (RecentFileItem r in Items.Values)
                    writer.WriteLine(r.ToXmlString());
                writer.WriteLine("</files>");
            }
        }
    }
}
