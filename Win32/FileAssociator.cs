﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Paulus.Win32
{
        //   private void RegisterExtension()
        //{
        //    try
        //    {
        //        FileAssociator.AssociateExtension(".click", Application.ExecutablePath, "clickfile", "Autoclick key combination file");
        //    }
        //    catch (UnauthorizedAccessException) //ok not enough privileges
        //    {
        //        //ok do nothing
        //    }
        //}
    public static class FileAssociator
    {
        //[System.Security.Permissions.RegistryPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
        public static bool AssociateExtension(string extension, string applicationPath, string identifier,
            string description, string icon)
        {
            try
            {

                RegistryKey CR = Registry.ClassesRoot;

                //CreateRegistryKey   HKEY_CLASSES_ROOT\Extension
                RegistryKey extensionKey = CR.CreateSubKey(extension);
                //SetRegistryValue of HKEY_CLASSES_ROOT\Extension, use default value, value= Identifier
                extensionKey.SetValue("", identifier, RegistryValueKind.String);
                extensionKey.Close();

                //CreateRegistryKey HKEY_CLASSES_ROOT\Identifier
                RegistryKey identifierKey = CR.CreateSubKey(identifier);
                //SetRegistryValue  HKEY_CLASSES_ROOT, Identifier, "", REG_SZ, Description
                identifierKey.SetValue("", description, RegistryValueKind.String);
                if (icon != "")
                {
                    //CreateRegistryKey HKEY_CLASSES_ROOT\Identifier\DefaultIcon
                    RegistryKey defaultIconKey = identifierKey.CreateSubKey("DefaultIcon");
                    //SetRegistryValue  of HKEY_CLASSES_ROOT\Identifier\DefaultIcon, use default value,value= Icon
                    defaultIconKey.SetValue("", icon); defaultIconKey.Close();
                }

                //Identifier = Identifier + "\shell"
                //CreateRegistryKey HKEY_CLASSES_ROOT\Identifier
                RegistryKey shellKey = identifierKey.CreateSubKey("shell");
                //Identifier = Identifier + "\open"
                //CreateRegistryKey HKEY_CLASSES_ROOT\Identifier
                RegistryKey openKey = shellKey.CreateSubKey("open");
                //Identifier = Identifier + "\command"
                //CreateRegistryKey HKEY_CLASSES_ROOT\Identifier
                RegistryKey commandKey = openKey.CreateSubKey("command");
                //SetRegistryValue  of HKEY_CLASSES_ROOT\Identifier, use default value,value= ("Application" "%1")
                commandKey.SetValue("", "\"" + applicationPath + "\" \"%1");
                commandKey.Close(); openKey.Close(); shellKey.Close();
                identifierKey.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AssociateExtension(string extension, string applicationPath, string identifier,
            string description)
        {
            return AssociateExtension(extension, applicationPath, identifier, description, "");
        }
    }
}
