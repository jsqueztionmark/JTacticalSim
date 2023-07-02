using System;
using System.Linq;
using System.IO;
using JTacticalSim.API.Component;
using JTacticalSim.Utility;

namespace JTacticalSim.DataContext
{
	internal class GameFileHandler<T> : IDataFileHandler<T>
	{
		public IDataFileInfo<T> DataFiles { get; private set;}
		
		public GameFileHandler(IDataFileInfo<T> dataFiles)
		{
			DataFiles = dataFiles;
		}

		public bool FileRootDirectoryExists() { return Directory.Exists(DataFiles.GameSaveRootDirectory); }
		public bool FileDirectoryExists() { return Directory.Exists(DataFiles.GameSaveDirectory); }
		public bool FileDirectoryHasFiles() { return Directory.GetFiles(DataFiles.GameSaveDirectory).Any(); }
		public bool ComponentDirectoryExists() { return Directory.Exists(DataFiles.ComponentDirectory); }
		public bool ComponentDirectoryHasFiles() { return Directory.GetFiles(DataFiles.ComponentDirectory).Any(); }


		public IResult<IDataFileHandler<T>, IDataFileHandler<T>> CreateNewFileDirectory()
		{
			var r = new DataResult<IDataFileHandler<T>, IDataFileHandler<T>> { Status = API.ResultStatus.SUCCESS};

			try
			{
				Directory.CreateDirectory(DataFiles.GameSaveDirectory);
				r.Messages.Add("Directory {0} created.".F(DataFiles.GameSaveDirectory)); 
				r.SuccessfulObjects.Add(this);
			}
			catch (Exception ex)
			{
				r.Status = API.ResultStatus.EXCEPTION;
				r.Messages.Add("Directory {0} was not created.".F(DataFiles.GameSaveDirectory));
				r.ex = ex;
				r.FailedObjects.Add(this);
			}			

			return r;
		}

		public IResult<IDataFileHandler<T>, IDataFileHandler<T>> DeleteFileDirectory()
		{
			var r = new DataResult<IDataFileHandler<T>, IDataFileHandler<T>> { Status = API.ResultStatus.SUCCESS };

			try
			{
				Directory.Delete(DataFiles.GameSaveRootDirectory, true);
				r.SuccessfulObjects.Add(this);
			}
			catch (Exception ex)
			{
				r.Status = API.ResultStatus.EXCEPTION;
				r.Messages.Add("Directory {0} was not deleted.".F(DataFiles.GameSaveRootDirectory));
				r.ex = ex;
				r.FailedObjects.Add(this);
			}			

			r.Messages.Add("Directory {0} deleted.".F(DataFiles.GameSaveRootDirectory));
			return r;

		}

		public IResult<FileInfo, FileInfo> CopyFiles(string from, string to)
		{
			var r = new DataResult<FileInfo, FileInfo> { Status = API.ResultStatus.SUCCESS };
			var fromDirectory = new DirectoryInfo(from);

			foreach (FileInfo fi in fromDirectory.GetFiles())
			{
				try
				{
					fi.CopyTo(Path.Combine(to, fi.Name));
					r.SuccessfulObjects.Add(fi);
				}
				catch (Exception ex)
				{
					r.Status = API.ResultStatus.EXCEPTION;
					r.Messages.Add("Files not copied.");
					r.ex = ex;
					r.FailedObjects.Add(fi);
					return r;
				}

			}

			r.Messages.Add("Data files copied.");
			return r;
		}

		public IResult<FileInfo, FileInfo> CopyFiles(string to)
		{
			var r = new DataResult<FileInfo, FileInfo> { Status = API.ResultStatus.SUCCESS };
			var fromDirectory = new DirectoryInfo(this.DataFiles.GameSaveDirectory);

			foreach (FileInfo fi in fromDirectory.GetFiles())
			{
				try
				{
					fi.CopyTo(Path.Combine(to, fi.Name));
					r.SuccessfulObjects.Add(fi);
				}
				catch (Exception ex)
				{
					r.Status = API.ResultStatus.EXCEPTION;
					r.Messages.Add("Files not copied.");
					r.ex = ex;
					r.FailedObjects.Add(fi);
					return r;
				}

			}

			r.Messages.Add("Scenario files copied.");
			return r;
		}
		

	}
}
