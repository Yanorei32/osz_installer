using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

class OSZInstaller {
	[DllImport("kernel32.dll")]
	static extern int GetPrivateProfileString(
		string lpApplicationName,
		string lpKeyName,
		string lpDefault,
		StringBuilder lpReturnedstring,
		int nSize,
		string lpFileName
	);

	class Configure {
		private string songsDir;
		private bool removeFileAfterInstall;

		public bool RemoveFileAfterInstall {
			get { return this.removeFileAfterInstall; }
		}

		public string SongsDir {
			get { return this.songsDir; }
		}

		public Configure (string songsDir, bool removeFileAfterInstall) {
			this.songsDir = songsDir;
			this.removeFileAfterInstall = removeFileAfterInstall;
		}
	}

	static string getIniValue(
		string path,
		string section,
		string key,
		string defaultValue= ""
	) {
		StringBuilder sb = new StringBuilder(256);

		GetPrivateProfileString(
			section,
			key,
			defaultValue,
			sb,
			sb.Capacity,
			path
		);

		return sb.ToString();
	}

	static bool getBoolByString(string str, bool defaultValue) {
		return (str == (!defaultValue ? "true" : "false")) ? !defaultValue : defaultValue;
	}

	static Configure getConfigureByIni() {
		string iniPath = Path.Combine(
			Directory.GetParent(
				Assembly.GetExecutingAssembly().Location
			).ToString(),
			"configure.ini"
		);

		string songsDir = getIniValue(
			iniPath,
			"basic",
			"songs",
			@"%appdata%\..\Local\osu!\Songs"
		);

		songsDir = Environment.ExpandEnvironmentVariables(songsDir);

		if (!Directory.Exists(songsDir))
			throw new DirectoryNotFoundException(
				"osu! beatmap directory not found."
			);

		bool removeFileAfterInstall= getBoolByString(
			getIniValue(
				iniPath,
				"advanced",
				"removeFileAfterInstall",
				"true"
			),
			true
		);

		return new Configure(
			songsDir,
			removeFileAfterInstall
		);
	}

	static void installOSZ(Configure c, string filePath) {
		string extension = Path.GetExtension(filePath);
		if (extension != ".osz")
			throw new Exception(
				"This file extension is not 'osz'."
			);


		string songName = Path.GetFileNameWithoutExtension(filePath);

		if (!File.Exists(filePath))
			throw new FileNotFoundException(
				"osu! beatmap file not found.",
				filePath
			);

		string songDirPath = Path.Combine(c.SongsDir, songName);

		if (Directory.Exists(songDirPath))
			throw new Exception(
				"osu! beatmap already installed."
			);

		Directory.CreateDirectory(songDirPath);

		try {
			ZipFile.ExtractToDirectory(
				filePath,
				songDirPath
			);
		} catch (Exception e) {
			Directory.Delete(songDirPath);
			throw e;
		}

		if (c.RemoveFileAfterInstall)
			File.Delete(filePath);

	}

	static void Main(string[] Args) {
		Configure c = getConfigureByIni();

		int processed = 0;
		foreach (string filePath in Args) {
			try {
				installOSZ(c, filePath);

			} catch (Exception e) {
				MessageBox.Show(string.Format(
					"Exception asserted in {0}\n----\n{1}",
					Path.GetFileName(filePath),
					e.ToString()
				));
				continue;
			}

			processed++;
		}

		MessageBox.Show(string.Format(
			(Args.Length - processed == 0) ? "{0} Processed normaly." : "{0} Processed normaly.\n{1} Error(s).",
			processed,
			Args.Length - processed
		));
	}
}

