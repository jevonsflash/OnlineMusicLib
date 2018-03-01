namespace OnlineMusic.Helper
{
  public class TextHelper
  {
    public static string XtoYGetTo(string all, string r, string l, int t)
    {

      int A = all.IndexOf(r, t);
      int B = all.IndexOf(l, A + 1);
      if (A < 0 || B < 0)
      {
        return null;
      }
      else
      {
        A = A + r.Length;
        B = B - A;
        if (A < 0 || B < 0)
        {
          return null;
        }
        return all.Substring(A, B);
      }
    }
  }

}
