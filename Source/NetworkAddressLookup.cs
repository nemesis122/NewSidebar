using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Aurora;

public record NetworkLookupResult(NetworkAddressState State,bool ExternalFailed);

// No timer and no startup hook. Only the explicit Settings button calls FetchAsync.
public sealed class NetworkAddressLookup
{
 private static readonly HttpClient DefaultClient=new(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(5)};
 private readonly HttpClient client;
 private readonly Func<string,string?> localAddress;
 private readonly TimeSpan timeout;
 public NetworkAddressLookup(HttpClient? client=null,Func<string,string?>? localAddress=null,TimeSpan? timeout=null){this.client=client??DefaultClient;this.localAddress=localAddress??ReadLocal;this.timeout=timeout??TimeSpan.FromSeconds(4);}
 public async Task<NetworkLookupResult> FetchAsync(string deviceId,NetworkAddressState? previous,bool includeExternal,CancellationToken cancellation=default)
 {
  cancellation.ThrowIfCancellationRequested();
  string? local=await Task.Run(()=>localAddress(deviceId),cancellation).ConfigureAwait(false);
  var state=(previous??new NetworkAddressState()) with{InternalIP=Normalize(local),InternalUpdated=DateTimeOffset.UtcNow};
  if(!includeExternal)return new(state,false);
  using var deadline=CancellationTokenSource.CreateLinkedTokenSource(cancellation);deadline.CancelAfter(timeout);
  try
  {
   using var response=await client.GetAsync("https://api64.ipify.org",HttpCompletionOption.ResponseHeadersRead,deadline.Token).ConfigureAwait(false);response.EnsureSuccessStatusCode();
   using var stream=await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);var bytes=new byte[129];int count=0;
   while(count<bytes.Length){int read=await stream.ReadAsync(bytes.AsMemory(count,bytes.Length-count),deadline.Token).ConfigureAwait(false);if(read==0)break;count+=read;}
   string? external=count<bytes.Length?Normalize(Encoding.UTF8.GetString(bytes,0,count)):null;
   if(external==null)throw new InvalidDataException("Invalid address response");
   return new(state with{ExternalIP=external,ExternalUpdated=DateTimeOffset.UtcNow},false);
  }
  catch(OperationCanceledException) when(cancellation.IsCancellationRequested){throw;}
  catch(Exception error) when(error is HttpRequestException or IOException or InvalidDataException or OperationCanceledException){return new(state,true);}
 }
 public static string? Normalize(string? value)=>IPAddress.TryParse(value?.Trim(),out var address)?address.ToString():null;
 private static string? ReadLocal(string deviceId)
 {
  string id=deviceId.StartsWith("network/",StringComparison.Ordinal)?deviceId[8..]:deviceId;
  var nic=NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n=>n.Id==id)??throw new InvalidOperationException("Network adapter is no longer available");
  var addresses=nic.GetIPProperties().UnicastAddresses.Select(a=>a.Address).Where(a=>!IPAddress.IsLoopback(a)).ToArray();
  return (addresses.FirstOrDefault(a=>a.AddressFamily==AddressFamily.InterNetwork&&!a.ToString().StartsWith("169.254.",StringComparison.Ordinal))??addresses.FirstOrDefault(a=>a.AddressFamily==AddressFamily.InterNetworkV6&&!a.IsIPv6LinkLocal)??addresses.FirstOrDefault())?.ToString();
 }
}
