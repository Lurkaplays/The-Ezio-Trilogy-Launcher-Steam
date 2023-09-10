﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Management;

// Imported
using Serilog;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows.Navigation;
using The_Ezio_Trilogy_Launcher.Windows.AC2_Pages;

namespace The_Ezio_Trilogy_Launcher.Windows
{
	/// <summary>
	/// Interaction logic for AssassinsCreed2.xaml
	/// </summary>

	public partial class AssassinsCreed2 : Window
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll")]
		public static extern IntPtr SetProcessAffinityMask(IntPtr hProcess, IntPtr dwProcessAffinityMask);

		// Path to the installation folder
		private string? path { get; set; }

		// Cache for all of the pages
		private Dictionary<string, Page> pageCache = new Dictionary<string, Page>();

		// This is to check if uMod is enabled or disabled
		public static bool uModStatus { get; set; }

		public AssassinsCreed2()
		{
			InitializeComponent();
			path = App.AC2Path;
		}

		private void NavigateToPage(string PageName)
		{
			Log.Information($"Trying to navigate to {PageName}");
			switch (PageName)
			{
				case "Credits":
					
                    if (!pageCache.ContainsKey(PageName))
                    {
						
                        Log.Information("Page is not cached. Loading it and caching it for future use.");
                        AC2_Pages.Credits page = new AC2_Pages.Credits();
                        pageCache[PageName] = page;
                        PageViewer.Content = pageCache[PageName];
                    }
                    else
                    {
                        Log.Information("Page is already cached. Loading it");
                        PageViewer.Content = pageCache[PageName];
                    }
                    break;
				case "Settings":
                    if (!pageCache.ContainsKey(PageName))
                    {
                        Log.Information("Page is not cached. Loading it and caching it for future use.");
                        AC2_Pages.Settings page = new AC2_Pages.Settings();
                        pageCache[PageName] = page;
                        PageViewer.Content = page;
                    }
                    else
                    {
                        Log.Information("Page is already cached. Loading it");
                        PageViewer.Content = pageCache[PageName];
                    }
                    break;
				case "Mods":
                    if (!pageCache.ContainsKey(PageName))
                    {
                        Log.Information("Page is not cached. Loading it and caching it for future use.");
                        AC2_Pages.Mods page = new AC2_Pages.Mods();
                        pageCache[PageName] = page;
                        PageViewer.Content = page;
                    }
                    else
                    {
                        Log.Information("Page is already cached. Loading it");
                        PageViewer.Content = pageCache[PageName];
                    }
                    break;
				default:
                    if (!pageCache.ContainsKey(PageName))
                    {
                        Log.Information("Page is not cached. Loading it and caching it for future use.");
                        AC2_Pages.Default_Page page = new AC2_Pages.Default_Page();
                        pageCache[PageName] = page;
                        PageViewer.Content = page;
                    }
					else
					{
                        Log.Information("Page is already cached. Loading it");
                        PageViewer.Content = pageCache[PageName];
					}
                    break;
			}
		}

        // Sets Process Affinity based on the amount of cores
        // According to PCGamingWiki it can help with Stutters and Tearing
        private async Task SetProcessAffinity(int gameProcessID)
		{
			try
			{
				Log.Information("Grabbing game process by ID to change affinity.");
				Process[] processes = Process.GetProcessesByName("AssassinsCreedIIGame");
				while (processes.Length <= 0)
				{
                    processes = Process.GetProcessesByName("AssassinsCreedIIGame");
					await Task.Delay(1000);
				}
				Log.Information($"Game process found.");
				int affinity;
				switch (true)
				{
					case bool when App.NumberOfCores >= 8 && App.NumberOfThreads >= 16:
						Log.Information("8 Cores/16 Threads or greater affinity");;
						foreach (Process gameProcess in processes)
						{
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
                            gameProcess.ProcessorAffinity = new IntPtr(0xFFFF);
                        }
						break;
					case bool when App.NumberOfCores == 6 && App.NumberOfThreads == 12:
						Log.Information("6 Cores/12 Threads affinity");
                        foreach (Process gameProcess in processes)
                        {
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
							gameProcess.ProcessorAffinity = new IntPtr(0x7F);
                        }
                        break;
					case bool when App.NumberOfCores == 6 && App.NumberOfThreads == 6:
						Log.Information("6 Cores/6 Threads affinity");
                        foreach (Process gameProcess in processes)
                        {
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
                            gameProcess.ProcessorAffinity = new IntPtr(0x3F);
                        }
                        break;
					case bool when App.NumberOfCores == 8 && App.NumberOfThreads == 8:
					case bool when App.NumberOfCores == 4 && App.NumberOfThreads == 8:
						Log.Information("4 Cores/8 Threads or 8 Cores/8 Threads affinity");
                        foreach (Process gameProcess in processes)
                        {
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
                            gameProcess.ProcessorAffinity = new IntPtr(0xFF);
                        }
                        break;
					case bool when App.NumberOfCores == 4 && App.NumberOfThreads == 4:
						Log.Information("4 Cores/4 Threads affinity");
                        foreach (Process gameProcess in processes)
                        {
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
                            gameProcess.ProcessorAffinity = new IntPtr(0x0F);
                        }
                        break;
					default:
						Log.Information("Default preset");
						affinity = (1 << App.NumberOfThreads) - 1;
						Log.Information($"Affinity Bitmask: 0x{affinity.ToString("X")}");
                        foreach (Process gameProcess in processes)
                        {
                            Log.Information($"Game Process: {gameProcess.ProcessName}, ID: {gameProcess.Id}");
                            gameProcess.ProcessorAffinity = new IntPtr(affinity);
                        }
                        break;
				}
				await Task.Delay(10);
			}
			catch (Exception ex)
			{
				Log.Information(ex, "Error:");
				return;
			}
		}

		// Window Dragging
		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				DragMove();
			}
		}

		// Exits the Launcher back to the main window
		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Log.Information("Closing Assassin's Creed 2 Launcher");
			this.Close();
		}

		// Starts the game
		private async void Play_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (uModStatus)
				{
                    // Create a new process start info
                    ProcessStartInfo GameInfo = new ProcessStartInfo
                    {
                        FileName = path + @"\AssassinsCreedIIGame.exe",
                        UseShellExecute = false, // Required for setting affinity
                        RedirectStandardOutput = false, // Set to true if you want to capture the output
                        CreateNoWindow = true // Set to true to hide the console window
                    };
                    Process uModProcess = new Process();
                    uModProcess.StartInfo.WorkingDirectory = path + @"\uMod";
                    uModProcess.StartInfo.FileName = "uMod.exe";
                    uModProcess.StartInfo.UseShellExecute = true;
                    uModProcess.Start();
                    Process gameProcess = new Process();
                    gameProcess.StartInfo = GameInfo;
                    gameProcess.Start();
                    Log.Information("Game is starting");
                    Log.Information("Setting game affinity based on CPU Core/Thread Count");
                    gameProcess.PriorityClass = ProcessPriorityClass.High;
                    await SetProcessAffinity(gameProcess.Id);
                    Log.Information("Game started");
                    string[] gameSpecificProcesses = { "UbisoftGameLauncher", "Steam", "AssassinsCreedIIGame" };
                    Log.Information($"Waiting for game to be closed.");
                    while (true)
                    {
                        int closedGameSpecificProcesses = 0;
                        foreach (string GameProcess in gameSpecificProcesses)
                        {
                            Process[] process = Process.GetProcessesByName(GameProcess);
                            if (process.Length <= 0)
                            {
                                closedGameSpecificProcesses++;
                            }
                            if (GameProcess == "AssassinsCreedIIGame" && process.Length <= 0)
                            {
                                break;
                            }
                        }
                        if (closedGameSpecificProcesses == 2)
                        {
                            Process[] process = Process.GetProcessesByName("AssassinsCreedIIGame");
                            if (process.Length <= 0)
                            {
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                    Log.Information("Game Closed");
                    try
                    {
                        if (Process.GetProcessById(uModProcess.Id) != null)
                        {
                            uModProcess.CloseMainWindow();
                        }
                    }
                    catch (Exception)
                    {
                        Log.Information("uMod is already closed.");
                    }
                    await Task.Delay(10);
                } 
                else
                {
                    // Create a new process start info
                    ProcessStartInfo GameInfo = new ProcessStartInfo
                    {
                        FileName = path + @"\AssassinsCreedIIGame.exe",
                        UseShellExecute = false, // Required for setting affinity
                        RedirectStandardOutput = false, // Set to true if you want to capture the output
                        CreateNoWindow = true // Set to true to hide the console window
                    };
                    Process gameProcess = new Process();
                    gameProcess.StartInfo = GameInfo;
                    gameProcess.Start();
                    Log.Information("Game is starting");
                    Log.Information("Setting game affinity based on CPU Core/Thread Count");
                    gameProcess.PriorityClass = ProcessPriorityClass.High;
                    await SetProcessAffinity(gameProcess.Id);
                    Log.Information("Game started");
                    string[] gameSpecificProcesses = { "UbisoftGameLauncher", "Steam", "AssassinsCreedIIGame" };
                    Log.Information($"Waiting for game to be closed.");
                    while (true)
                    {
                        int closedGameSpecificProcesses = 0;
                        foreach (string GameProcess in gameSpecificProcesses)
                        {
                            Process[] process = Process.GetProcessesByName(GameProcess);
                            if (process.Length <= 0)
                            {
                                closedGameSpecificProcesses++;
                            }
                            if (GameProcess == "AssassinsCreedIIGame" && process.Length <= 0)
                            {
                                break;
                            }
                        }
                        if (closedGameSpecificProcesses == 2)
                        {
                            Process[] process = Process.GetProcessesByName("AssassinsCreedIIGame");
                            if (process.Length <= 0)
                            {
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                    Log.Information("Game Closed");
                    await Task.Delay(10);
                }
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error:");
				return;
			}
		}

		private void Credits_Click(object sender, RoutedEventArgs e)
		{
			NavigateToPage("Credits");
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ubisoft\Assassin's Creed 2\Assassin2.ini"))
			{
				Log.Information("Game configuration file found");
                NavigateToPage("Settings");
            }
			else
			{
                Log.Information("Game configuration not found");
				System.Windows.MessageBox.Show("Game configuration not found. Please open the game once and change some setting in game to generate that file fully to be able to change settings here.");
				return;
            }
        }

		private void Update_Click(object sender, RoutedEventArgs e)
		{
            NavigateToPage("Update");
        }

        private void uMod_Click(object sender, RoutedEventArgs e)
        {
			NavigateToPage("Mods");
        }
    }
}