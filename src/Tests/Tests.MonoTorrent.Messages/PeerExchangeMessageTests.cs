﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using MonoTorrent.BEncoding;

using NUnit.Framework;

namespace MonoTorrent.Messages.Peer.Libtorrent
{
    [TestFixture]
    public class PeerExchangeMessageTests
    {
        [Test]
        public void ParametersAreEmpty_Ctor ()
        {
            var x = new PeerExchangeMessage ();
            Assert.IsTrue (x.Added.IsEmpty);
            Assert.IsTrue (x.AddedDotF.IsEmpty);
            Assert.IsTrue (x.Dropped.IsEmpty);
        }

        [Test]
        public void ReleaseRentedMessage ()
        {
            (var message, var releaser) = PeerMessage.Rent<PeerExchangeMessage> ();
            message.Initialize (
                new ExtensionSupports (new[] { PeerExchangeMessage.Support }),
                new byte[14],
                new byte[2],
                new byte[31],
                default
            );
            
            Assert.AreEqual (14, message.Added.Length);
            Assert.AreEqual (2, message.AddedDotF.Length);
            Assert.AreEqual (31, message.Dropped.Length);

            releaser.Dispose ();
            Assert.IsTrue (message.Added.IsEmpty);
            Assert.IsTrue (message.AddedDotF.IsEmpty);
            Assert.IsTrue (message.Dropped.IsEmpty);
        }


        [Test]
        public void PeerExchangeMessageDecode ()
        {
            // Decodes as: 192.168.0.1:100
            byte[] peer = { 192, 168, 0, 1, 100, 0 };
            byte[] peerDotF = { 1 | 2 }; // 1 == encryption, 2 == seeder

            var supports = new ExtensionSupports (new[] { PeerExchangeMessage.Support });
            PeerExchangeMessage message = new PeerExchangeMessage ().Initialize (supports, peer, peerDotF, default, default);

            byte[] buffer = message.Encode ();
            PeerExchangeMessage m = (PeerExchangeMessage) PeerMessage.DecodeMessage (buffer, null).message;
            Assert.IsTrue (peer.AsSpan ().SequenceEqual (m.Added.Span), "#1");
            Assert.IsTrue (peerDotF.AsSpan ().SequenceEqual (m.AddedDotF.Span), "#2");
        }

        [Test]
        public void PeerExchangeMessageDecode_Empty ()
        {
            var data = new BEncodedDictionary ().Encode ();
            var message = new PeerExchangeMessage ();
            message.Decode (data.AsSpan ());
            Assert.IsTrue (message.Added.IsEmpty, "#1");
            Assert.IsTrue (message.AddedDotF.IsEmpty, "#2");
            Assert.IsTrue (message.Dropped.IsEmpty, "#3");
        }
    }
}
