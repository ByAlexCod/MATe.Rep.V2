﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MATeV2;
using System.Threading;
using System.IO.Compression;

namespace Network
{
    public class SyncerReceiver : IUseNetwork
    {
        IPAddress _ip;
        int _port;
        TcpListener _listener;
        string _tempReceiverPath;
        string _contextStoragePath;
        string _tempUnZipped;
        ContextAndUserManager _baseCtxUser;
        public static bool _newReceive = false;

        
        public SyncerReceiver(IPAddress ip, int port, string contextesStoragePath, string tempReceiverPath, string tempUnZipped, ContextAndUserManager baseCtxUser)
        {
            if (!Directory.Exists(contextesStoragePath)) Directory.CreateDirectory(contextesStoragePath);
            if (!Directory.Exists(tempUnZipped)) Directory.CreateDirectory(tempUnZipped);
            _contextStoragePath = contextesStoragePath;
            _tempReceiverPath = tempReceiverPath;
            _tempUnZipped = tempUnZipped;
            _baseCtxUser = baseCtxUser;
            _ip = ip;
            _port = port;
            _listener = new TcpListener(ip, port);
        }
        
        public void Start()
        {
            _listener.Start();
            while (true)
            {
                using (var client = _listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                using (FileStream output = File.Open(_tempReceiverPath, FileMode.OpenOrCreate))
                {
                    Console.WriteLine("Client connected. Starting to receive 0the file");
                    // read the file in chunks of 1KB
                    var buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
                Thread a = new Thread(new ThreadStart(End));
                a.IsBackground = true;
                a.Start();
            }
        }

        void End()
        {
            if( Directory.Exists( _tempUnZipped ) ) Directory.Delete(_tempUnZipped, true);
            Directory.CreateDirectory(_tempUnZipped);

            using (ZipArchive arch = ZipFile.OpenRead(_tempReceiverPath))
            {
                arch.ExtractToDirectory(_tempUnZipped);
            }

            File.Delete(_tempReceiverPath);
            DirectoryInfo d = new DirectoryInfo(_tempUnZipped);

            using (var ct = _baseCtxUser.ObtainAccessor())
            {
                Context b = ct.Context;
                foreach (var unZippedFile in d.GetFiles("*.MATe"))
                {
                    bool replace = false;

                    if (File.Exists(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName)))
                    {
                        ContextAndUserManager unZippedContext = new ContextAndUserManager(b.CompanyName, true);
                        unZippedContext.Load(unZippedFile.FullName);

                        ContextAndUserManager existingContext = new ContextAndUserManager(b.CompanyName, true);
                        existingContext.Load(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));


                        using (var uzc = unZippedContext.ObtainAccessor())
                        {
                            using (var ec = existingContext.ObtainAccessor())
                            {
                                if (uzc.Context.ModifyDate >= ec.Context.ModifyDate) replace = true;
                                b.Merge(uzc.Context);
                            }
                        }
                        if (replace == true)
                        {
                            File.Delete(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                            File.Copy(unZippedFile.FullName, _contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                        }
                    }
                    else
                    {
                        File.Copy(unZippedFile.FullName, _contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                    }


                    ContextAndUserManager existingContext1 = new ContextAndUserManager(b.CompanyName, true);
                    existingContext1.Load(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                    using (var cte = existingContext1.ObtainAccessor())
                    {
                        b.Merge(cte.Context);
                        _newReceive = true;
                    }

                    //ContextAndUserManager ctxuser = new ContextAndUserManager(b.CompanyName, true);
                    //ctxuser.Load(unZippedFile.FullName);
                    //if (File.Exists(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName)))
                    //{
                    //    ContextAndUserManager before = new ContextAndUserManager(b.CompanyName, true);
                    //    before.Load(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));

                    //    using (var beforeuh = before.ObtainAccessor())
                    //    using (var after = ctxuser.ObtainAccessor())
                    //    {
                    //        Context beforeuuh = beforeuh.Context;
                    //        Context aftereuh = after.Context;

                    //        if (aftereuh.ModifyDate > beforeuuh.ModifyDate)
                    //        {
                    //            File.Delete(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                    //            unZippedFile.CopyTo(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                    //        b.Merge(before, aftereuh);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    unZippedFile.CopyTo(_contextStoragePath + @"\" + Path.GetFileName(unZippedFile.FullName));
                    //}
                }


            }
        }

        public bool IsNewSync()
        {
            return _newReceive;
        }
    }
}
