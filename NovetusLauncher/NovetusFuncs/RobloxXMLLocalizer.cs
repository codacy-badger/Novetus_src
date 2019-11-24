﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

public static class RobloxXMLLocalizer
{
    public enum DLType
    {
        //RBXL and RBXM
        RBXL,
        RBXM,
        //Items
        Hat,
        Head,
        Face,
        TShirt,
        Shirt,
        Pants
    }

    public static void DownloadFromNodes(string filepath, AssetCacheDef assetdef)
    {
        DownloadFromNodes(filepath, assetdef.Class, assetdef.Id[0], assetdef.Ext[0], assetdef.Dir[0], assetdef.GameDir[0]);
    }

    public static void DownloadFromNodes(string filepath, AssetCacheDef assetdef, int idIndex, int extIndex, int outputPathIndex, int inGameDirIndex)
    {
        DownloadFromNodes(filepath, assetdef.Class, assetdef.Id[idIndex], assetdef.Ext[extIndex], assetdef.Dir[outputPathIndex], assetdef.GameDir[inGameDirIndex]);
    }

    public static void DownloadFromNodes(string filepath, string itemClassValue, string itemIdValue, string fileext, string outputPath, string inGameDir)
    {
        string oldfile = File.ReadAllText(filepath);
        string fixedfile = RemoveInvalidXmlChars(ReplaceHexadecimalSymbols(oldfile));
        XDocument doc = XDocument.Parse(fixedfile);

        try
        {
            var v = from nodes in doc.Descendants("Item")
                    where nodes.Attribute("class").Value == itemClassValue
                    select nodes;

            foreach (var item in v)
            {
                var v2 = from nodes in item.Descendants("Content")
                         where nodes.Attribute("name").Value == itemIdValue
                         select nodes;

                foreach (var item2 in v2)
                {
                    var v3 = from nodes in item2.Descendants("url")
                             select nodes;

                    foreach (var item3 in v3)
                    {
                        if (!item3.Value.Contains("rbxasset"))
                        {
                            //do whatever with your value
                            string url = item3.Value;
                            string urlFixed = url.Replace("&amp;", "&").Replace("amp;", "&");
                            //MessageBox.Show(urlFixed, "Novetus Asset Localizer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            string peram = "id=";

                            if (urlFixed.Contains(peram))
                            {
                                string IDVal = urlFixed.After(peram);
                                DownloadFilesFromNode(urlFixed, outputPath, fileext, IDVal);
                                item3.Value = inGameDir + IDVal + fileext;
                            }
                        }
                    }
                }
            }

        }
        catch (Exception ex) when (!Env.Debugging)
        {
            MessageBox.Show("The download has experienced an error: " + ex.Message, "Novetus Asset Localizer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            doc.Save(filepath);
        }
    }

    private static void DownloadFilesFromNode(string url, string path, string fileext, string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            Downloader download = new Downloader(url, id);

            try
            {
                download.InitDownload(path, fileext);
            }
            catch (Exception ex) when (!Env.Debugging)
            {
                MessageBox.Show("The download has experienced an error: " + ex.Message, "Novetus Asset Localizer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private static string RemoveInvalidXmlChars(string content)
    {
        return new string(content.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray());
    }

    private static string ReplaceHexadecimalSymbols(string txt)
    {
        string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
        return Regex.Replace(txt, r, "", RegexOptions.Compiled);
    }
}