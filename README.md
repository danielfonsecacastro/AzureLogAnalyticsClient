# AzureLogAnalyticsClient

## Como usar

```var logger = new Log("workspaceId", "sharedKey");
_ = await logger.Error(new
{
    Message = "Sua mensagem de erro"
});
```
