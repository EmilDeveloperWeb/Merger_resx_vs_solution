using MergeRESX.Helpers;
using MergeRESX.Models;
using System.Diagnostics.Metrics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace MergeResx
{
    class Program
    {
        static void Main(string[] args)
        {
            var workingFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "resx");
            var folders = ConfigDialog(workingFolder);
            foreach (var aFolder in folders) { ExecuteTheMerge(aFolder, aFolder); }
        }


        static void ExecuteTheMerge(string folderName, string Destinationfolder)
        {
            string[] AllfileName = Directory.GetFiles(folderName).Where(x=> x.EndsWith(".resx")).ToArray();

            XMLMerger Merger = new XMLMerger();

            IEnumerable<Resource> allResourcesClean =
              from f in AllfileName
              let doc = XDocument.Load(f)
              from e in doc.Root!.Elements("data")
              select Resource.Parse(e, f);

            
            // Add the name of the file in node and get all nodes of all files of folderName
            var output = Merger.MergeAllNodes(allResourcesClean);

            // replace $this For <filename>.Text
            output = Merger.AddFileNamesInNodeValueThis(output);


            // remove all nodes without the .Text in name
            //output = Merger.RemoveOthersThanTextNodes(output);


            output = Merger.RemoveRepeatedNodes(output);

            // replace next repeated id(name) to the file name it belongs...
            output = Merger.AddFileNameToRepeatedIds(output);
            
            
            // removed repeated id(names) 
            // output = Merger.RemoveNodesWithRepeatedName(output, folderName);

            // get a list dictionary in .txt of repeated values
            _ = Merger.DetermineRepeatedValues(output!, folderName);

            // create the resx and save
            var resx = Merger.FormatFile(output);
            resx.Save(Destinationfolder + "/Merge.resx");



            //----------------------------------------------------------//

            Console.WriteLine("\nMerged files in: " + folderName + "\n\n-------\n");

        }




        static List<string> ConfigDialog(string Destinationfolder, string lang = "")
        {

            Console.WriteLine("Working folder:\n" + Destinationfolder);
            Console.WriteLine("Input the root to read the .resx files or enter to use default folder...\n");
            var newPath = Console.ReadLine();

            try
            {
                Path.GetFullPath(newPath!);
                Destinationfolder = newPath!;
                Console.WriteLine("Working folder had been set To: \n" + Destinationfolder + "\n\n");
            }
            catch { }


            if (!Directory.Exists(Destinationfolder))
            {
                Directory.CreateDirectory(Destinationfolder);
            }


            string[] AllfileName = Directory.GetFiles(Destinationfolder);


            Console.Clear();

            string[] languages = new string[] { };

            if (lang == "")
            {
                Console.WriteLine("Spesify languages separated by comma, or enter to generate all, example: ru, es, fr, en, etc...:");
                languages = Console.ReadLine()!.Split(",");
            }
            else
            {
                languages = lang.Split(",");
            }



            List<string> folders_lang = new List<string>();

            foreach (var l in languages)
            {
                string aFolderPath_DESTINATION = Destinationfolder + "\\" + l + "_lang".Trim();

                Directory.CreateDirectory(aFolderPath_DESTINATION);


                var group = AllfileName.Where(fs => fs.IndexOf(l.Trim() + ".resx") != -1).ToList();

                foreach (var file in group)
                {
                    File.Move(file, aFolderPath_DESTINATION + "\\" + Path.GetFileName(file));
                }

                folders_lang.Add(aFolderPath_DESTINATION);
            }

            return folders_lang;
        }

    }

}
