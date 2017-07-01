using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using micro_transfer_check;

namespace microtransfercheck.Migrations
{
    [DbContext(typeof(TransferJobDBContext))]
    [Migration("20170701155633_FirstOne")]
    partial class FirstOne
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("micro_transfer_check.OrderAwaitingTransfer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("OrdersAwaitingTransfer");
                });
        }
    }
}
