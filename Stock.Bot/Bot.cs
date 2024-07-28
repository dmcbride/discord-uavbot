namespace Stock.Bot;

class Bot : Discord.Extensions.Bot
{
  public Bot() : base(Environment.GetEnvironmentVariable("stock_secret")!)
  {

  }
}
