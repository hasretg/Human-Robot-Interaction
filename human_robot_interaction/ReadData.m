

%%Read the data into a 3D array
Raw = dlmread(['Raw0.txt'],';');
Raw = reshape(Raw',22,6,size(Raw,1)/6);
Raw = permute(Raw,[2,1,3]);
%Raw(1,:,:) = Raw(1,:,:) - 40;

Filtered = dlmread(['Filtered0.txt'],';');
Filtered = reshape(Filtered',22,6,size(Filtered,1)/6);
Filtered = permute(Filtered,[2,1,3]);

Predicted = dlmread(['Predicted0.txt'],';');
Predicted = reshape(Predicted',22,6,size(Predicted,1)/6);
Predicted = permute(Predicted,[2,1,3]);

%% Calculate the means
MeanRaw = squeeze(mean(Raw,2));
MeanFiltered = squeeze(mean(Filtered,2));
MeanPredicted = squeeze(mean(Predicted,2));

%% Extract the index finger tip
IndexTipRaw = squeeze(Raw(:,10,:));
IndexTipFiltered = squeeze(Filtered(:,10,:));
IndexTipPredicted = squeeze(Predicted(:,10,:));

%% Plot the raw and the filtered data in 3D
% figure()
% plot3(IndexTipRaw(1,:),IndexTipRaw(2,:),IndexTipRaw(3,:))
% hold on
% plot3(IndexTipFiltered(1,:),IndexTipFiltered(2,:),IndexTipFiltered(3,:))
% hold off

%% Plot the raw and the filtered data in 2D space
% figure()
% axis on
% 
% subplot(2,2,1);
% plot(IndexTipRaw(1,:),IndexTipRaw(2,:))
% hold on
% plot(IndexTipFiltered(1,:),IndexTipFiltered(2,:));
% title('xy plane')
% 
% subplot(2,2,2);
% plot(IndexTipRaw(1,:),IndexTipRaw(3,:))
% hold on
% plot(IndexTipFiltered(1,:),IndexTipFiltered(3,:));
% title('xz plane')
% 
% subplot(2,2,3);
% plot(IndexTipRaw(2,:),IndexTipRaw(3,:))
% hold on
% plot(IndexTipFiltered(2,:),IndexTipFiltered(3,:));
% title('yz plane')
% leg = legend('Raw Measurements','Filtered Measurements')
% 
% sub = subplot(2,2,4);
% axis off
% set(leg, 'position',get(sub,'position'))
% hold off


%% Calculate and plot absolute position

NormRaw = (sum(IndexTipRaw(1:3,:).^2)).^0.5;
NormFiltered = (sum(IndexTipFiltered(1:3,:).^2)).^0.5;
NormPredicted = (sum(IndexTipPredicted(1:3,:).^2)).^0.5;

figure()

plot((1:length(NormRaw))/50,NormRaw)
% title('Tracking and filtering of the index finger (tip)')
xlabel('Time [s]')
ylabel('Distance to camera [cm]')
hold on
plot((1:length(NormFiltered))/50,NormFiltered)
legend('Raw Coordinates', 'Filtered Coordinates')
% plot(1:length(NormPredicted),NormPredicted)
hold off
%% Calculate deviation
Mean = squeeze(mean(Raw,3));
std1 = zeros(6,22);
for i = 1:size(Raw,3)
    std1 = std1 + (Mean - squeeze(Raw(:,:,i))).^2;
end
std1 = std1./(size(Raw,3)-1);
std1 = std1.^0.5;
std1 = mean(std1,2)

std2 = mean(std(Filtered,0,3),2)

