using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCC
{
    public enum OperationType : uint { Transaction = 1, ChangeKey, RecoverFounds, ListAccountForSale,
        DeListAccountForSale, BuyAccount, ChangeKeySigned, ChangeAccountInfo };

    public class OperationBlock
    {
        public OperationBlock(Stream s)
        {
            using(BinaryReader br  = new BinaryReader(s, System.Text.Encoding.ASCII, true))
            {
                Soob = br.ReadByte();
                if (Soob > 0)
                {
                    ProtocolVersion = br.ReadUInt16();
                    AvailableProtocol = br.ReadUInt16();
                }
                BlockNumber = br.ReadUInt32();
                AccountKey = new ECKeyPair();
                AccountKey.LoadFromStream(s);
                Reward = br.ReadUInt64();
                Fee = br.ReadUInt64();
                Timestamp = br.ReadUInt32();
                CompactTarget = br.ReadUInt32();
                Nonce = br.ReadUInt32();
                ushort pl = br.ReadUInt16();
                if (pl > 0)
                {
                    Payload = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    SafeBoxHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    OperationHash = br.ReadBytes(pl);
                }
                pl = br.ReadUInt16();
                if (pl > 0)
                {
                    ProofOfWork = br.ReadBytes(pl);
                }
            }
        }

        public OperationBlock()
        {

        }

        public byte Soob { get; set; } = 3;
        public ushort ProtocolVersion { get; set; } = 0;
        public ushort AvailableProtocol { get; set; } = 0;
        public uint BlockNumber { get; set; } = 0;
        public ECKeyPair AccountKey { get; set; } = new ECKeyPair();
        public UInt64 Reward { get; set; } = 0;
        public UInt64 Fee { get; set; } = 0;
        public uint Timestamp { get; set; } = 0;
        public uint CompactTarget { get; set; } = 0;
        public uint Nonce { get; set; } = 0;
        public byte[] Payload { get; set; }
        public string PayloadString
        {
            get
            {
                return Encoding.UTF8.GetString(Payload);
            }
        }
        public byte[] SafeBoxHash { get; set; }
        public byte[] OperationHash { get; set; }
        public byte[] ProofOfWork { get; set; }

        virtual public void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, System.Text.Encoding.ASCII, true))
            {
                bw.Write(Soob);
                bw.Write(ProtocolVersion);
                bw.Write(AvailableProtocol);
                bw.Write(BlockNumber);
                if (AccountKey == null)
                {
                    bw.Write((ushort)6);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                    bw.Write((ushort)0);
                }
                else
                {
                    AccountKey.SaveToStream(s);
                }
                bw.Write(Reward);
                bw.Write(Fee);
                bw.Write(Timestamp);
                bw.Write(CompactTarget);
                bw.Write(Nonce);
                if (Payload != null)
                {
                    bw.Write((ushort)Payload.Length);
                    bw.Write(Payload);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (SafeBoxHash != null)
                {
                    bw.Write((ushort)SafeBoxHash.Length);
                    bw.Write(SafeBoxHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (OperationHash != null)
                {
                    bw.Write((ushort)OperationHash.Length);
                    bw.Write(OperationHash);
                }
                else
                {
                    bw.Write((ushort)0);
                }
                if (ProofOfWork != null)
                {
                    bw.Write((ushort)ProofOfWork.Length);
                    bw.Write(ProofOfWork);
                }
                else
                {
                    bw.Write((ushort)0);
                }
            }
        }

    }

    public class Transaction
    {
        public uint Sender { get; set; }
        public uint NumberOfOperations { get; set; }
        public uint Target { get; set; }
        public ulong Amount { get; set; }
        public ulong Fee { get; set; }
        public byte[] Payload { get; set; }
        public ECSig Signature { get; set; }
	public ulong AccountPrice { get; set; }
	public uint SellerAccount { get; set; }
	public ECKeyPair NewAccountKey { get; set; }
        public ECKeyPair AccountKey { get; set; }

        public string PayloadString {
            get {
                return Encoding.ASCII.GetString(Payload);
            }
        }

        public Transaction(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                Sender = br.ReadUInt32();
                NumberOfOperations = br.ReadUInt32();
                Target = br.ReadUInt32();
                Amount = br.ReadUInt64();
                Fee = br.ReadUInt64();
                ushort PayloadLength = br.ReadUInt16();
                Payload = new byte[PayloadLength];
                br.Read(Payload, 0, PayloadLength); // 372
	        try{
		    //ushort u = br.ReadUInt16();
		    //Console.WriteLine(u);
    	    	    //stream.Position-=2;
                    AccountKey = new ECKeyPair();
            	    AccountKey.LoadFromStream(stream, false);
		} catch (Exception e) {
			Console.WriteLine(PayloadLength);
			Console.WriteLine("Error {0} {1} {2}",Sender,Target,Amount);
			throw e;
		}
                byte b = br.ReadByte();
                if (b > 2){ stream.Position -= 1; }
		if (b > 0 && b<3) {
		    try{
			AccountPrice = br.ReadUInt64();
			SellerAccount = br.ReadUInt32();
    			NewAccountKey = new ECKeyPair();
    			NewAccountKey.LoadFromStream(stream, false);
    			Console.WriteLine("Account Operation: Price: {0}, Seller: {1}, B:{2}, newKey {3}", AccountPrice, SellerAccount, b, Encoding.ASCII.GetString(NewAccountKey.pub.X.GetEncoded()) );
		    } catch (Exception e){
			Console.WriteLine("B: {0}",b);
			throw e;
		    }
		}
                Signature = new ECSig(stream);
            }
        }

        public void SaveToStream(Stream s)
        {
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write(Sender);
                bw.Write(NumberOfOperations);
                bw.Write(Target);
                bw.Write(Amount);
                bw.Write(Fee);
                bw.Write((ushort)Payload.Length);
                bw.Write(Payload);
                AccountKey.SaveToStream(s,false);
                Signature.SaveToStream(s);
            }
        }
    }

    public class ChangeKeyTransaction {
	public uint SignerAccount { get; set; }
	public uint TargetAccount { get; set; }
	public uint NumberOfOperations { get; set; }
	public ulong Fee { get; set; }
	public byte[] Payload { get; set; }
	public ECKeyPair PublicKey { get; set; }
	public ECKeyPair NewAccountKey { get ;set; }
	public ECSig Signature { get ;set; }

	public ChangeKeyTransaction(){}

	public ChangeKeyTransaction(Stream s, OperationType OpType){
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true)){
		SignerAccount = br.ReadUInt32();
		if(OpType == OperationType.ChangeKey) {
		     TargetAccount = SignerAccount;
		}
		else if(OpType == OperationType.ChangeKeySigned) {
		     TargetAccount = br.ReadUInt32();
		}
		NumberOfOperations = br.ReadUInt32();
		Fee = br.ReadUInt64();
		ushort len = br.ReadUInt16();
		Payload = new byte[len];
		br.Read(Payload, 0, len);
		PublicKey = new ECKeyPair();
		try{
		    PublicKey.LoadFromStream(s, false);
		}catch(Exception e){
		    Console.WriteLine("PublicKey");
		    Console.ReadLine();
		    throw e;
		}
		NewAccountKey = new ECKeyPair();
		try{
		    NewAccountKey.LoadFromStream(s, true);
		    //Console.WriteLine("OK");
		}catch(Exception e){
		    Console.WriteLine("NewAccountKey");
		    Console.ReadLine();
		    throw e;
		}
		Signature = new ECSig(s);
	    }
	}
    }

    public class Operation : OperationBlock
    {
        public uint OpCount { get; set; }
        public OperationType OpType { get; set; }
        public List<Transaction> Transactions;
        public Operation(Stream s) :base(s)
        {            
            using (BinaryReader br = new BinaryReader(s, Encoding.Default, true))
            {
                OpCount = br.ReadUInt32();
                if (OpCount > 0)
                {
                    Transactions = new List<Transaction>();
                    for (int i = 0; i < OpCount; i++)
                    {
                        OpType = (OperationType)br.ReadUInt32();
                        if (OpType == OperationType.Transaction || OpType == OperationType.BuyAccount)
                        {
			    try{
                                Transaction t = new Transaction(s);
                                Transactions.Add(t);
				if(t.NewAccountKey!=null)
        	            	    Console.WriteLine("Block {0}, new Transaction {1} => {2} {3}", BlockNumber, t.Sender, t.Target, t.Amount);
			    }catch(Exception e){
				Console.WriteLine(OpType);
				Console.WriteLine(BlockNumber);
				throw e;
			    }
                        }
			else if(OpType == OperationType.ChangeKey || OpType == OperationType.ChangeKeySigned) {
			    ChangeKeyTransaction ct = new ChangeKeyTransaction(s, OpType);
//			    Console.WriteLine("Change key Signer: {0} Target: {1} ", ct.SignerAccount, ct.TargetAccount);
			}
                        else
                        {
			    Console.WriteLine(OpType);
                            s.Position = s.Length;
                            return;
                        }
                    }
                }
            }
        }

        public override void SaveToStream(Stream s)
        {
            base.SaveToStream(s);
            using (BinaryWriter bw = new BinaryWriter(s, Encoding.ASCII, true))
            {
                bw.Write((uint)Transactions.Count);
                foreach(var t in Transactions)
                {
                    t.SaveToStream(s);
                }
            }
        }

    }
}