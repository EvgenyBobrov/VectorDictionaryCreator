using Android.App;
using Android.Widget;
using Android.OS;
using VectorDataLayer;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace VectorDictionaryCreator
{
	[Activity (Label = "VectorDictionaryCreator", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		private ListView _fileListView;
		private int _curentPosition = -1;

		private TextView _infoField;

		private const string DICT_LIST_MODE = "dict_list";
		private const string WORK_LIST_MODE = "work_list";
		private string _mode = string.Empty;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			_infoField = FindViewById<EditText> (Resource.Id.mlFileText);

			// Get our button from the layout resource,
			// and attach an event to it
			Button createDictButton = FindViewById<Button> (Resource.Id.btnCreateDict);
			createDictButton.Click += delegate
			{
				CreateDictionaries();
			};

			Button dictListButton = FindViewById<Button> (Resource.Id.btnDictList	);
			dictListButton.Click += delegate
			{
				_mode = DICT_LIST_MODE;
				ShowDictFiles();
				_infoField.Text = string.Empty;
			};

			Button workListButton = FindViewById<Button> (Resource.Id.btnWorkList);
			workListButton.Click += delegate
			{
				_mode = WORK_LIST_MODE;
				ShowWorkingFiles();
				_infoField.Text = string.Empty;
			};

			_fileListView = FindViewById<ListView> (Resource.Id.lvFiles);
			_fileListView.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) =>
			{
				//ShowItem (fileListView.SelectedItem.ToString());
				ShowItem (_fileListView.GetItemAtPosition(e.Position).ToString());
				_curentPosition = e.Position;
			};


			_fileListView.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
			{
				return;
			};

			Button delFileButton = FindViewById<Button> (Resource.Id.btnDelFile);
			delFileButton.Click += delegate
			{
				DelFile();
			};

			Button createScheduleButton = FindViewById<Button> (Resource.Id.btnCreateSchedule);
			createScheduleButton.Click += delegate
			{
				CreateSchedule();
			};

			Button saveFileButton = FindViewById<Button> (Resource.Id.btnSaveFile);
			saveFileButton.Click += delegate
			{
				SaveFile();
			};
		}

		public override void OnWindowFocusChanged (bool hasFocus)
		{
			base.OnWindowFocusChanged (hasFocus);
			if (hasFocus)
				SetView();
		}


		private void CreateDictionaries ()
		{
			var dictionaryDirectory = CreateDictionaryDirectory ();
			var bSuccess = true;
			if (!string.IsNullOrEmpty (dictionaryDirectory))
			{
				bSuccess = CreateEmpoees (dictionaryDirectory);

				bSuccess = bSuccess && CreateWinches (dictionaryDirectory);
			}

			return;
		}

		private bool CreateSchedule ()
		{
			var filePath = Path.Combine (FileDataAccessor.GetVectorDataDirectory (), DictionaryFilePrefix.SCHEDULE_FILE_NAME);
			var testDataAccessor = new TestDataAccessor ();
			var scheduleItems = testDataAccessor.GetSchedule (System.DateTime.Today);
			var schedule = new FlightSchedule (){ Paradroms = scheduleItems };
			var fileContent = MySerializer.Serialize (schedule);
			File.WriteAllText (filePath, fileContent);
			/*var files = Directory.EnumerateFiles (dictionaryDirectory + Path.DirectorySeparatorChar)
				.FirstOrDefault();
			var name = Path.GetFileName (files);*/
			return true;
		}

		private bool CreateEmpoees (string dictionaryDirectory)
		{
			var filePath = Path.Combine (dictionaryDirectory, DictionaryFilePrefix.EMPLOEE_FILE_NAME);
			var testDataAccessor = new TestDataAccessor ();
			var emploees = testDataAccessor.GetEmploees();
			var emploeeList = new EmploeeList (){ Emploees = emploees };
			var fileContent = MySerializer.Serialize (emploeeList);
			File.WriteAllText (filePath, fileContent);
			/*var files = Directory.EnumerateFiles (dictionaryDirectory + Path.DirectorySeparatorChar)
				.FirstOrDefault();
			var name = Path.GetFileName (files);*/
			return true;
		}

		private bool CreateWinches (string dictionaryDirectory)
		{
			var filePath = Path.Combine (dictionaryDirectory, DictionaryFilePrefix.WINCH_FILE_NAME);
			var testDataAccessor = new TestDataAccessor ();
			var winches = testDataAccessor.GetWinches();
			var winchList = new WinchList (){ Winches = winches };
			var fileContent = MySerializer.Serialize (winchList);
			File.WriteAllText (filePath, fileContent);
			/*var files = Directory.EnumerateFiles (dictionaryDirectory + Path.DirectorySeparatorChar)
				.FirstOrDefault();
			var name = Path.GetFileName (files);*/
			return true;
		}

		private string CreateDictionaryDirectory()
		{
			var path = Android.OS.Environment.ExternalStorageDirectory;
			var dir = Path.Combine(path.Path, DictionaryFilePrefix.VECTOR_DIRECTORY);
			if (!Directory.Exists (dir))
			{
				Directory.CreateDirectory (dir);
			}

			dir = Path.Combine (dir, DictionaryFilePrefix.DICTIONARY_DIRECTORY);

			if (!Directory.Exists (dir))
			{
				Directory.CreateDirectory (dir);
			}

			return dir;
		}


		private void ShowWorkingFiles()
		{
			var path = FileDataAccessor.GetVectorDataDirectory ();
			var fileList = Directory.EnumerateFiles (path + Path.DirectorySeparatorChar)
				.ToList ();
			var spList = new List<string> ();
			foreach (var file in fileList)
			{
				var fileName = Path.GetFileName (file);
				spList.Add (fileName);
			}

			var fileListAdapter = new ArrayAdapter<string> (this, 
				Android.Resource.Layout.SimpleListItemActivated1, 
				spList.ToArray ());
			
			_fileListView.Adapter = fileListAdapter;
			_curentPosition = -1;
		}

		private void ShowDictFiles()
		{
			var dir = CreateDictionaryDirectory();
			var fileList = Directory.EnumerateFiles (dir + Path.DirectorySeparatorChar)
				.ToList ();
			var spList = new List<string> ();
			foreach (var file in fileList)
			{
				var fileName = Path.GetFileName (file);
				spList.Add (fileName);
			}

			var fileListAdapter = new ArrayAdapter<string> (this, 
				Android.Resource.Layout.SimpleListItemActivated1, 
				spList.ToArray ());

			_fileListView.Adapter = fileListAdapter;
			_curentPosition = -1;
		}

		private void ShowItem(string fileName)
		{
			var path = string.Empty;
			if (_mode == WORK_LIST_MODE)
				path = Path.Combine (FileDataAccessor.GetVectorDataDirectory (), fileName);
			else if (_mode == DICT_LIST_MODE)
				path = Path.Combine (CreateDictionaryDirectory (), fileName);
			else
				return;
			
			var content = File.ReadAllText (path);
			_infoField.Text = content;
			//var workingDay = MySerializer.Deserialize<WorkingDay> (content);
			//_infoField.Text = workingDay.FlightDirector.Name;
		}

		private void DelFile()
		{
			var data = string.Empty;
			var path = string.Empty;
			if (_curentPosition >= 0)
			{
				var selectedFile = _fileListView.
					Adapter.GetItem(_curentPosition).ToString ();
				if (_mode == DICT_LIST_MODE)
				{
					path = Path.Combine (CreateDictionaryDirectory (), selectedFile);
					File.Delete (path);
					ShowDictFiles ();
					data = "File deleted";
				}
				else if (_mode == WORK_LIST_MODE)
				{
					path = Path.Combine (FileDataAccessor.GetVectorDataDirectory (), selectedFile);
					File.Delete (path);
					ShowWorkingFiles ();
					data = "File deleted";
				}
				else
					data = "Wrong mode";

			}
			else
				data = "Item doesn't selected";

			_infoField.Text = data;
		}

		private void SaveFile()
		{
			var data = string.Empty;
			var path = string.Empty;
			if (_curentPosition >= 0)
			{
				var selectedFile = _fileListView.
					Adapter.GetItem(_curentPosition).ToString ();
				if (_mode == DICT_LIST_MODE)
				{
					path = Path.Combine (CreateDictionaryDirectory (), selectedFile);
					File.WriteAllText (path, _infoField.Text);
					ShowDictFiles ();
					data = "File saved";
				}
				else if (_mode == WORK_LIST_MODE)
				{
					path = Path.Combine (FileDataAccessor.GetVectorDataDirectory (), selectedFile);
					File.WriteAllText (path, _infoField.Text);
					ShowWorkingFiles ();
					data = "File saved";
				}
				else
					data = "Wrong mode";

			}
			else
				data = "Item doesn't selected";

			_infoField.Text = data;
		}
		private void SetView ()
		{
			var headerLayout = FindViewById<LinearLayout> (Resource.Id.linearLayout1);
			var leftHeaderLayout = FindViewById<LinearLayout> (Resource.Id.linearLayout2);
			var par =  new LinearLayout.LayoutParams(headerLayout.Width / 2, headerLayout.Height);
			leftHeaderLayout.LayoutParameters = par;
			var metrics = Resources.DisplayMetrics;
			var totalHeight = metrics.WidthPixels / metrics.Density;
			var lvHeight = (int)(totalHeight - headerLayout.Height) / 2;
			_fileListView.LayoutParameters = 
				new LinearLayout.LayoutParams (_fileListView.Width, lvHeight);
			/*
			var ll3 = FindViewById<LinearLayout> (Resource.Id.linearLayout3);
			var buttonDictList = FindViewById<Button> (Resource.Id.btnDictList);
			buttonDictList.LayoutParameters = new LinearLayout.LayoutParams (ll3.Width , ll3.Height / 2);

			var buttonLayout = FindViewById<LinearLayout> (Resource.Id.llButtons);
			buttonLayout.LayoutParameters = new LinearLayout.LayoutParams (ll3.Width , ll3.Height / 2);

			var btnDel = FindViewById<Button> (Resource.Id.btnDelFile);
			btnDel.LayoutParameters = new LinearLayout.LayoutParams (buttonLayout.Width / 2, buttonLayout.Height);
			var btnSave = FindViewById<Button> (Resource.Id.btnSaveFile);
			btnSave.LayoutParameters = new LinearLayout.LayoutParams (buttonLayout.Width / 2, buttonLayout.Height);
			*/
		}
	}
}


