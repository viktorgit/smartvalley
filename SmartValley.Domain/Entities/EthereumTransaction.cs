﻿using System;

namespace SmartValley.Domain.Entities
{
    public class EthereumTransaction
    {
        public EthereumTransaction(
            long userId,
            string hash,
            EthereumTransactionType type,
            DateTimeOffset created)
        {
            UserId = userId;
            Hash = hash;
            Type = type;
            Status = EthereumTransactionStatus.InProgress;
            Created = created;
        }

        // ReSharper disable once UnusedMember.Local
        private EthereumTransaction()
        {
        }

        public long Id { get; set; }

        public long UserId { get; set; }

        public string Hash { get; set; }

        public EthereumTransactionType Type { get; set; }

        public EthereumTransactionStatus Status { get; set; }

        public DateTimeOffset Created { get; set; }

        public User User { get; set; }

        public void Complete()
            => SetStatus(EthereumTransactionStatus.Completed);

        public void Fail()
            => SetStatus(EthereumTransactionStatus.Failed);

        private void SetStatus(EthereumTransactionStatus status)
        {
            if (Status != EthereumTransactionStatus.InProgress)
                throw new InvalidOperationException($"Status of completed transaction '{Hash}' cannot be changed");

            Status = status;
        }
    }
}