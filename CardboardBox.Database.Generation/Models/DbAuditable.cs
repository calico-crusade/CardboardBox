namespace CardboardBox.Database.Generation
{
	public abstract class DbAuditable : DbObject
	{
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
