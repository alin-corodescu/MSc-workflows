package dataplanesvc

import "context"

type DataPlaneService interface {
	// register a new piece of metadata in the cluster
	PostMetadata(ctx context.Context, payload MetadataPayload) error

	// gets the metadata identified by an id
	GetMetadataById(ctx context.Context, ID string) (MetadataPayload, error)
}

// payload for the metadata service
type MetadataPayload struct {
	ID string `json:"id"`
	Localization LocalizationInformation `json:"localizationInformation"`
	Metadata string `json:"metadata"`
}

// contains information about the localization of the data that was just published
type LocalizationInformation struct {
	NodeId string `json:"nodeId"`
	Region string `json:"Region"`
}

type InMemMetadataCatalogService struct {
	catalog map[string]MetadataPayload
}

func MakeInMemMetadataCatalogService() InMemMetadataCatalogService {
	return InMemMetadataCatalogService{
		catalog: make(map[string]MetadataPayload),
	}
}
func (s InMemMetadataCatalogService) PostMetadata(ctx context.Context, payload MetadataPayload) error {
	s.catalog[payload.ID] = payload
	return nil
}

func (s InMemMetadataCatalogService) GetMetadataById(ctx context.Context, ID string) (MetadataPayload, error) {
	// todo add error cases
	payload := s.catalog[ID]
	return payload, nil
}
