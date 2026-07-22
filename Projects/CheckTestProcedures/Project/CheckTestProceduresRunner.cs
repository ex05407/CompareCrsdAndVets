using CheckTestProcedures.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CheckTestProcedures.Project
{
    internal sealed class CheckTestProceduresRunner
    {
        public int Run(CommandLineOptions options)
        {
            if (options == null)
            {
                return 1;
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string allConfigPath = Path.Combine(baseDirectory, "Config", "config_All.xml");
            string selectedConfigPath = string.IsNullOrWhiteSpace(options.FormatFilePath)
                ? Path.Combine(baseDirectory, "Config", "config_Selected.xml")
                : options.FormatFilePath;

            ConfigReader configReader = new ConfigReader();
            InputResolver inputResolver = new InputResolver();
            ZipExtractor zipExtractor = new ZipExtractor();
            ResourceReader resourceReader = new ResourceReader();
            TsvWriter tsvWriter = new TsvWriter();

            ConfigAllSettings allSettings = configReader.LoadAll(allConfigPath);
            ConfigSelectedSettings selectedSettings = configReader.LoadSelected(selectedConfigPath);
            InputResolution resolution = inputResolver.Resolve(options.InputPath);

            string outputBaseName = inputResolver.ResolveOutputBaseName(options.InputPath, options.OutputFilePath, resolution);
            string outputFolder = Path.Combine(baseDirectory, "Output");
            Directory.CreateDirectory(outputFolder);

            IReadOnlyList<string> sourceFiles = resolution.SourceFiles;
            List<string> filesToRead = new List<string>();
            List<string> cleanupPaths = new List<string>();

            if (resolution.Kind == InputKind.ZipArchive || resolution.Kind == InputKind.FolderWithZip)
            {
                foreach (string archivePath in sourceFiles)
                {
                    IReadOnlyList<string> extractedFiles;
                    string cleanupPath = zipExtractor.ExtractWorkingFolder(archivePath, out extractedFiles);
                    cleanupPaths.Add(cleanupPath);
                    filesToRead.Add(cleanupPath);
                }
            }
            else
            {
                filesToRead.AddRange(sourceFiles);
            }

            try
            {
                List<ExtractedResource> resources = resourceReader.Read(filesToRead, resolution, allSettings);
                string allTsvPath = Path.Combine(outputFolder, outputBaseName + "_All.tsv");
                string selectedTsvPath = Path.Combine(outputFolder, outputBaseName + "_Selected.tsv");

                tsvWriter.WriteAll(allTsvPath, resources);
                tsvWriter.WriteSelected(selectedTsvPath, resources, selectedSettings);
                Console.WriteLine(allTsvPath);
                Console.WriteLine(selectedTsvPath);
                return 0;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
            catch (InvalidDataException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 3;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 4;
            }
            finally
            {
                foreach (string cleanupPath in cleanupPaths)
                {
                    if (!string.IsNullOrWhiteSpace(cleanupPath) && Directory.Exists(cleanupPath))
                    {
                        try
                        {
                            Directory.Delete(cleanupPath, true);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}
