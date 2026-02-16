using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AesProject.Core.Entities
{
    [Table("aes_encryption_meta_info")]
    public class AesEncryptionMetaInfo
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("secret_key")]
        public required byte[] SecretKey { get; set; }

        [Column("iv")]
        public required byte[] Iv { get; set; }

        [Column("tag_length")]
        public int TagLength { get; set; }
    }
}
