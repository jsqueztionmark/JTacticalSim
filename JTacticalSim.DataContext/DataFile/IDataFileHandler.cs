using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JTacticalSim.API.Component;
using JTacticalSim.API.Data;


namespace JTacticalSim.DataContext
{
	public interface IDataFileHandler<T>
	{
		IDataFileInfo<T> DataFiles { get; }

		bool FileRootDirectoryExists();
		bool FileDirectoryExists();
		bool FileDirectoryHasFiles();
		bool ComponentDirectoryExists();
		bool ComponentDirectoryHasFiles();

		IResult<IDataFileHandler<T>, IDataFileHandler<T>> CreateNewFileDirectory();
		IResult<IDataFileHandler<T>, IDataFileHandler<T>> DeleteFileDirectory();

		/// <summary>
		/// Copies files from the current instance to a specified folder
		/// </summary>
		/// <param name="to"></param>
		/// <returns></returns>
		IResult<FileInfo, FileInfo> CopyFiles(string to);

		/// <summary>
		/// Copies files from a specified folder to another specified folder
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		IResult<FileInfo, FileInfo> CopyFiles(string from, string to);
	}
}
