using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;

// ReSharper disable All

namespace SOTFEdit.Infrastructure;

//Source: https://stackoverflow.com/questions/11624298/how-do-i-use-openfiledialog-to-select-a-folder
public class FolderPicker
{
    private const int ErrorCancelled = unchecked((int)0x800704C7);
    public virtual string? ResultPath { get; protected set; }
    public virtual string? ResultName { get; protected set; }
    public virtual string? InputPath { get; set; }
    public virtual bool ForceFileSystem { get; set; }
    public virtual string? Title { get; set; }
    public virtual string? OkButtonLabel { get; set; }
    public virtual string? FileNameLabel { get; set; }

    protected virtual int SetOptions(int options)
    {
        if (ForceFileSystem)
        {
            options |= (int)Fos.FosForcefilesystem;
        }

        return options;
    }

    // for WPF support
    public bool? ShowDialog(Window? owner = null, bool throwOnError = false)
    {
        owner ??= Application.Current.MainWindow;
        return ShowDialog(owner != null ? new WindowInteropHelper(owner).Handle : IntPtr.Zero, throwOnError);
    }

    // for all .NET
    public virtual bool? ShowDialog(IntPtr owner, bool throwOnError = false)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var dialog = (IFileOpenDialog)new FileOpenDialog();
        if (!string.IsNullOrEmpty(InputPath))
        {
            if (CheckHr(SHCreateItemFromParsingName(InputPath, null, typeof(IShellItem).GUID, out var item),
                    throwOnError) != 0)
            {
                return null;
            }

            dialog.SetFolder(item);
        }

        var options = Fos.FosPickfolders;
        options = (Fos)SetOptions((int)options);
        dialog.SetOptions(options);

        if (Title != null)
        {
            dialog.SetTitle(Title);
        }

        if (OkButtonLabel != null)
        {
            dialog.SetOkButtonLabel(OkButtonLabel);
        }

        if (FileNameLabel != null)
        {
            dialog.SetFileName(FileNameLabel);
        }

        if (owner == IntPtr.Zero)
        {
            owner = Process.GetCurrentProcess().MainWindowHandle;
            if (owner == IntPtr.Zero)
            {
                owner = GetDesktopWindow();
            }
        }

        var hr = dialog.Show(owner);
        if (hr == ErrorCancelled)
        {
            return null;
        }

        if (CheckHr(hr, throwOnError) != 0)
        {
            return null;
        }

        if (CheckHr(dialog.GetResult(out var result), throwOnError) != 0)
        {
            return null;
        }

        if (CheckHr(result.GetDisplayName(Sigdn.SigdnDesktopabsoluteparsing, out var path), throwOnError) != 0)
        {
            return null;
        }

        ResultPath = path;

        if (CheckHr(result.GetDisplayName(Sigdn.SigdnDesktopabsoluteediting, out path), false) == 0)
        {
            ResultName = path;
        }

        return true;
    }

    private static int CheckHr(int hr, bool throwOnError)
    {
        if (hr != 0)
        {
            if (throwOnError)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        return hr;
    }

    [DllImport("shell32")]
    private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string? pszPath,
        IBindCtx? pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

    [DllImport("user32")]
    private static extern IntPtr GetDesktopWindow();

    [ComImport]
    [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
    private class FileOpenDialog
    {
    }

    [ComImport]
    [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig]
        int Show(IntPtr parent); // IModalWindow

        [PreserveSig]
        int SetFileTypes(); // not fully defined

        [PreserveSig]
        int SetFileTypeIndex(int iFileType);

        [PreserveSig]
        int GetFileTypeIndex(out int piFileType);

        [PreserveSig]
        int Advise(); // not fully defined

        [PreserveSig]
        int Unadvise();

        [PreserveSig]
        int SetOptions(Fos fos);

        [PreserveSig]
        int GetOptions(out Fos pfos);

        [PreserveSig]
        int SetDefaultFolder(IShellItem psi);

        [PreserveSig]
        int SetFolder(IShellItem psi);

        [PreserveSig]
        int GetFolder(out IShellItem ppsi);

        [PreserveSig]
        int GetCurrentSelection(out IShellItem ppsi);

        [PreserveSig]
        int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string? pszName);

        [PreserveSig]
        int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

        [PreserveSig]
        int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string? pszTitle);

        [PreserveSig]
        int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string? pszText);

        [PreserveSig]
        int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        [PreserveSig]
        int GetResult(out IShellItem ppsi);

        [PreserveSig]
        int AddPlace(IShellItem psi, int alignment);

        [PreserveSig]
        int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

        [PreserveSig]
        int Close(int hr);

        [PreserveSig]
        int SetClientGuid(); // not fully defined

        [PreserveSig]
        int ClearClientData();

        [PreserveSig]
        int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);

        [PreserveSig]
        int GetResults([MarshalAs(UnmanagedType.IUnknown)] out object ppenum);

        [PreserveSig]
        int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig]
        int BindToHandler(); // not fully defined

        [PreserveSig]
        int GetParent(); // not fully defined

        [PreserveSig]
        int GetDisplayName(Sigdn sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string? ppszName);

        [PreserveSig]
        int GetAttributes(); // not fully defined

        [PreserveSig]
        int Compare(); // not fully defined
    }

#pragma warning disable CA1712 // Do not prefix enum values with type name
    private enum Sigdn : uint
    {
        SigdnDesktopabsoluteediting = 0x8004c000,

        SigdnDesktopabsoluteparsing = 0x80028000
        /*
        SigdnFilesyspath = 0x80058000,
        SigdnNormaldisplay = 0,
        SigdnParentrelative = 0x80080001,
        SigdnParentrelativeediting = 0x80031001,
        SigdnParentrelativeforaddressbar = 0x8007c001,
        SigdnParentrelativeparsing = 0x80018001,
        SigdnUrl = 0x80068000*/
    }

    [Flags]
    private enum Fos
    {
        /*
        FosOverwriteprompt = 0x2,
        FosStrictfiletypes = 0x4,
        FosNochangedir = 0x8,*/
        FosPickfolders = 0x20,

        FosForcefilesystem = 0x40
        /*FosAllnonstorageitems = 0x80,
        FosNovalidate = 0x100,
        FosAllowmultiselect = 0x200,
        FosPathmustexist = 0x800,
        FosFilemustexist = 0x1000,
        FosCreateprompt = 0x2000,
        FosShareaware = 0x4000,
        FosNoreadonlyreturn = 0x8000,
        FosNotestfilecreate = 0x10000,
        FosHidemruplaces = 0x20000,
        FosHidepinnedplaces = 0x40000,
        FosNodereferencelinks = 0x100000,
        FosOkbuttonneedsinteraction = 0x200000,
        FosDontaddtorecent = 0x2000000,
        FosForceshowhidden = 0x10000000,
        FosDefaultnominimode = 0x20000000,
        FosForcepreviewpaneon = 0x40000000,
        FosSupportstreamableitems = unchecked((int)0x80000000)*/
    }
#pragma warning restore CA1712 // Do not prefix enum values with type name
}